using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace COG.Plugin.CSharp;

public static class CSharpPluginLoader
{
    public static readonly List<Assembly> LoadedPluginAssemblies = new();
    private static readonly Dictionary<string, List<Assembly>> ResourceAssemblies = new();
    private static readonly Dictionary<string, IPluginResourceIO> ResourceIOs = new();

    public static IPluginResourceIO GetResourceIO(string resourceId)
    {
        ResourceIOs.TryGetValue(resourceId, out var io);
        return io!;
    }

    public static bool LoadPlugin(LoadedResource resource)
    {
        if (resource.Archive == null)
        {
            resource.Status = ResourceLoadStatus.Failed;
            resource.ErrorMessage = "Zip archive is not open";
            return false;
        }

        try
        {
            resource.Status = ResourceLoadStatus.Loading;
            var assemblies = new List<Assembly>();

            var resourceIO = new PluginResourceIO(resource.ResourceId, resource.Archive);
            ResourceIOs[resource.ResourceId] = resourceIO;
            PluginResourceIO.RegisterResource(resource);

            var dllAssemblies = LoadDllsFromZip(resource);
            if (dllAssemblies.Count > 0)
            {
                assemblies.AddRange(dllAssemblies);
                Main.Logger.LogInfo($"[CSharpPlugin] {resource.ResourceId}: {dllAssemblies.Count} DLL(s) loaded");
            }

            var scriptAssembly = CompileAndLoadScripts(resource);
            if (scriptAssembly != null)
            {
                assemblies.Add(scriptAssembly);
            }

            if (assemblies.Count == 0)
            {
                resource.Status = ResourceLoadStatus.Failed;
                resource.ErrorMessage = "No DLL or compilable C# sources found in Scripts/";
                return false;
            }

            PluginContext.Current = resourceIO;
            foreach (var asm in assemblies)
                RegisterPluginAssembly(asm, resource);
            PluginContext.Current = null!;

            resource.LoadedAssemblies = assemblies;
            ResourceAssemblies[resource.ResourceId] = assemblies;
            LoadedPluginAssemblies.AddRange(assemblies);

            resource.Status = ResourceLoadStatus.Loaded;
            Main.Logger.LogInfo($"[CSharpPlugin] {resource.ResourceId}: {assemblies.Count} assembly(s) loaded");
            return true;
        }
        catch (System.Exception ex)
        {
            PluginContext.Current = null!;
            resource.Status = ResourceLoadStatus.Failed;
            resource.ErrorMessage = ex.ToString();
            Main.Logger.LogError($"[CSharpPlugin] {resource.ResourceId} failed: {ex}");
            return false;
        }
    }

    private static List<Assembly> LoadDllsFromZip(LoadedResource resource)
    {
        var assemblies = new List<Assembly>();
        var dllEntries = resource.Archive!.Entries
            .Where(e => !string.IsNullOrEmpty(e.Name)
                        && e.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                        && LoadedResource.NormalizePath(e.FullName).StartsWith("Scripts/", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var entry in dllEntries)
        {
            try
            {
                using var stream = entry.Open();
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                var assembly = AssemblyLoadContext.Default.LoadFromStream(new MemoryStream(ms.ToArray()));
                assemblies.Add(assembly);
                Main.Logger.LogInfo($"[CSharpPlugin] Loaded DLL: {entry.Name}");
            }
            catch (System.Exception ex)
            {
                Main.Logger.LogError($"[CSharpPlugin] Failed to load DLL '{entry.Name}': {ex.Message}");
            }
        }

        return assemblies;
    }

    private static Assembly CompileAndLoadScripts(LoadedResource resource)
    {
        var csEntries = resource.Archive!.Entries
            .Where(e => !string.IsNullOrEmpty(e.Name)
                        && e.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                        && LoadedResource.NormalizePath(e.FullName).StartsWith("Scripts/", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (csEntries.Count == 0) return null!;

        var sources = new Dictionary<string, string>();
        foreach (var entry in csEntries)
        {
            try
            {
                using var stream = entry.Open();
                using var reader = new StreamReader(stream);
                sources[entry.Name] = reader.ReadToEnd();
            }
            catch (System.Exception ex)
            {
                Main.Logger.LogError($"[CSharpPlugin] Failed to read '{entry.Name}': {ex.Message}");
            }
        }

        if (sources.Count == 0) return null!;

        return ScriptCompiler.Compile(sources, resource);
    }

    private static void RegisterPluginAssembly(Assembly assembly, LoadedResource resource)
    {
        try
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attr = method.GetCustomAttribute<PluginModuleInitializerAttribute>();
                    if (attr != null)
                    {
                        try { method.Invoke(null, null); }
                        catch (System.Exception ex)
                        {
                            Main.Logger.LogError($"[CSharpPlugin] Init failed {type.FullName}.{method.Name}: {ex.Message}");
                        }
                    }
                }

                try { System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle); }
                catch { }
            }

            Main.Logger.LogInfo($"[CSharpPlugin] Assembly registered: {assembly.GetName().Name}");
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"[CSharpPlugin] Failed to register assembly: {ex.Message}");
        }
    }

    public static void UnloadPlugin(string resourceId)
    {
        if (!ResourceAssemblies.TryGetValue(resourceId, out var assemblies)) return;

        foreach (var asm in assemblies)
            LoadedPluginAssemblies.Remove(asm);

        ResourceAssemblies.Remove(resourceId);
        Main.Logger.LogInfo($"[CSharpPlugin] {resourceId} unloaded");
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class PluginModuleInitializerAttribute : Attribute
{
}
