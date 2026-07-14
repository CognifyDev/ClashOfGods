using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace COG.Plugin.CSharp;

public static class ScriptCompiler
{
    private static string? _cscPath;

    private static bool FindCsc()
    {
        if (_cscPath != null) return true;

        var roots = new[]
        {
            @"C:\Program Files\dotnet",
            @"C:\Program Files (x86)\dotnet"
        };

        foreach (var root in roots)
        {
            var sdk = Path.Combine(root, "sdk");
            if (!Directory.Exists(sdk)) continue;

            var csc = Directory.GetDirectories(sdk)
                .Select(d => Path.Combine(d, "Roslyn", "bincore", "csc.dll"))
                .Where(File.Exists)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (csc != null)
            {
                _cscPath = csc;
                Main.Logger.LogInfo($"[ScriptCompiler] C# compiler: {_cscPath}");
                return true;
            }
        }

        Main.Logger.LogWarning("[ScriptCompiler] dotnet SDK with csc.dll not found");
        return false;
    }

    public static Assembly Compile(Dictionary<string, string> sources, LoadedResource resource)
    {
        Main.Logger.LogInfo($"[ScriptCompiler] Compiling {resource.ResourceId}, {sources.Count} source(s)");

        if (!FindCsc())
        {
            resource.Status = ResourceLoadStatus.Failed;
            resource.ErrorMessage = "dotnet SDK not found";
            return null!;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"cog_plugin_{resource.ResourceId}");
        try
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Directory.CreateDirectory(tempDir);

            foreach (var (name, code) in sources)
                File.WriteAllText(Path.Combine(tempDir, name), code);

            var outputDll = Path.Combine(tempDir, "plugin.dll");
            var rspPath = Path.Combine(tempDir, "build.rsp");

            var refPaths = CollectReferences();
            var rspLines = new List<string>
            {
                "/target:library",
                "/optimize",
                "/nologo",
                $"/out:\"{outputDll}\""
            };

            foreach (var r in refPaths)
                rspLines.Add($"/reference:\"{r}\"");

            foreach (var (n, _) in sources)
                rspLines.Add($"\"{Path.Combine(tempDir, n)}\"");

            File.WriteAllLines(rspPath, rspLines);

            Main.Logger.LogInfo($"[ScriptCompiler] {refPaths.Count} references, {sources.Count} sources");

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{_cscPath}\" @\"{rspPath}\"",
                WorkingDirectory = tempDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                resource.Status = ResourceLoadStatus.Failed;
                resource.ErrorMessage = "Failed to start csc";
                return null!;
            }

            process.WaitForExit(60000);

            if (process.ExitCode != 0 || !File.Exists(outputDll))
            {
                var stderr = process.StandardError.ReadToEnd();
                var stdout = process.StandardOutput.ReadToEnd();
                resource.Status = ResourceLoadStatus.Failed;
                resource.ErrorMessage = $"Compilation failed (exit={process.ExitCode}):\n{stderr}\n{stdout}";
                Main.Logger.LogError($"[ScriptCompiler] {resource.ErrorMessage}");
                return null!;
            }

            var dllBytes = File.ReadAllBytes(outputDll);
            var assembly = Assembly.Load(dllBytes);
            Main.Logger.LogInfo($"[ScriptCompiler] Compiled and loaded: {resource.ResourceId}");
            return assembly;
        }
        catch (System.Exception ex)
        {
            resource.Status = ResourceLoadStatus.Failed;
            resource.ErrorMessage = $"Compilation error: {ex}";
            Main.Logger.LogError($"[ScriptCompiler] {resource.ErrorMessage}");
            return null!;
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    private static List<string> CollectReferences()
    {
        var refPaths = new List<string>();

        var dotnetRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(
            Path.GetDirectoryName(Path.GetDirectoryName(_cscPath!)))))!;
        var refPack = Path.Combine(dotnetRoot, "packs", "Microsoft.NETCore.App.Ref");
        if (Directory.Exists(refPack))
        {
            var netRef = Directory.GetDirectories(refPack)
                .Select(d => Path.Combine(d, "ref", "net6.0"))
                .Where(Directory.Exists)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            if (netRef != null)
                refPaths.AddRange(Directory.GetFiles(netRef, "*.dll"));
        }

        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "System", "Microsoft", "mscorlib", "netstandard", "Il2CppSystem"
        };

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (asm.IsDynamic || string.IsNullOrEmpty(asm.Location)) continue;
            if (!File.Exists(asm.Location)) continue;

            var name = asm.GetName().Name;
            if (string.IsNullOrEmpty(name)) continue;
            if (excluded.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase))) continue;

            refPaths.Add(asm.Location);
        }

        return refPaths;
    }
}
