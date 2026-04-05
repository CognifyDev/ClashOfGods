using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using COG.Cosmetics.Hats;
using COG.Cosmetics.Nameplates;
using COG.Cosmetics.Visors;

namespace COG.Cosmetics;

public sealed class ZipPackageLoader
{
    private readonly HatLoader       _hatLoader;
    private readonly VisorLoader     _visorLoader;
    private readonly NameplateLoader _nameplateLoader;

    public ZipPackageLoader(HatLoader hatLoader, VisorLoader visorLoader, NameplateLoader nameplateLoader)
    {
        _hatLoader       = hatLoader;
        _visorLoader     = visorLoader;
        _nameplateLoader = nameplateLoader;
    }
    
    public void LoadAllPackages(string zipDirectory, string cacheDirectory)
    {
        // 修复：若装扮目录尚未创建，先补建，再扫描，避免 Directory.GetFiles 抛出
        // DirectoryNotFoundException（该异常会被上层 catch 捕获，但会导致所有包都无法加载）
        if (!Directory.Exists(zipDirectory))
        {
            Main.Logger.LogInfo($"[Cosmetics] Cosmetics directory not found, creating: {zipDirectory}");
            Directory.CreateDirectory(zipDirectory);
        }

        if (!Directory.Exists(cacheDirectory))
        {
            Directory.CreateDirectory(cacheDirectory);
        }

        var zips = Directory.GetFiles(zipDirectory, "*.zip", SearchOption.TopDirectoryOnly);
        if (zips.Length == 0)
        {
            Main.Logger.LogInfo("[Cosmetics] No zip packages found.");
            return;
        }

        foreach (var zip in zips)
        {
            try   { ProcessPackage(zip, cacheDirectory); }
            catch (System.Exception e)
            {
                Main.Logger.LogError(
                    $"[Cosmetics] Failed to process '{Path.GetFileName(zip)}':\n{e}");
            }
        }
    }

    private void ProcessPackage(string zipPath, string cacheRoot)
    {
        var name      = Path.GetFileNameWithoutExtension(zipPath);
        var cacheDir  = Path.Combine(cacheRoot, name);

        Main.Logger.LogInfo($"[Cosmetics] Processing package '{name}'.");

        if (!IsCacheValid(cacheDir, zipPath))
        {
            Main.Logger.LogInfo($"[Cosmetics]   Extracting '{name}'...");
            ExtractPackage(zipPath, cacheDir);
        }
        else
        {
            Main.Logger.LogInfo($"[Cosmetics]   Cache hit for '{name}', skipping extraction.");
        }

        var info = ReadPackageInfo(cacheDir, name);
        Main.Logger.LogInfo($"[Cosmetics]   {info.Name} v{info.Version} by {info.Author}");

        var hatsDir       = Path.Combine(cacheDir, "Hats");
        var visorsDir     = Path.Combine(cacheDir, "Visors");
        var nameplatesDir = Path.Combine(cacheDir, "NamePlates");

        if (Directory.Exists(hatsDir))       _hatLoader.LoadCosmetics(hatsDir, info.Author);
        if (Directory.Exists(visorsDir))     _visorLoader.LoadCosmetics(visorsDir, info.Author);
        if (Directory.Exists(nameplatesDir)) _nameplateLoader.LoadCosmetics(nameplatesDir, info.Author);
    }
    
    private static bool IsCacheValid(string cacheDir, string zipPath)
    {
        var jsonPath = Path.Combine(cacheDir, "cosmetics.json");
        if (!Directory.Exists(cacheDir) || !File.Exists(jsonPath)) return false;

        return File.GetLastWriteTimeUtc(jsonPath) >= File.GetLastWriteTimeUtc(zipPath);
    }

    private static void ExtractPackage(string zipPath, string cacheDir)
    {
        if (Directory.Exists(cacheDir))
            Directory.Delete(cacheDir, recursive: true);
        Directory.CreateDirectory(cacheDir);

        using var archive = ZipFile.OpenRead(zipPath);
        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name)) continue;

            var full = entry.FullName.Replace('\\', '/');
            
            if (full.Equals("cosmetics.json", StringComparison.OrdinalIgnoreCase))
            {
                entry.ExtractToFile(Path.Combine(cacheDir, "cosmetics.json"), overwrite: true);
                continue;
            }

            if (!entry.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;

            string? sub = null;
            if      (full.StartsWith("Hats/",       StringComparison.OrdinalIgnoreCase)) sub = "Hats";
            else if (full.StartsWith("Visors/",     StringComparison.OrdinalIgnoreCase) ||
                     full.StartsWith("Visions/",    StringComparison.OrdinalIgnoreCase)) sub = "Visors";
            else if (full.StartsWith("NamePlates/", StringComparison.OrdinalIgnoreCase)) sub = "NamePlates";

            if (sub == null) continue;

            var subDir = Path.Combine(cacheDir, sub);
            Directory.CreateDirectory(subDir);
            entry.ExtractToFile(Path.Combine(subDir, entry.Name), overwrite: true);
        }
    }

    private static PackageInfo ReadPackageInfo(string cacheDir, string fallbackName)
    {
        var path = Path.Combine(cacheDir, "cosmetics.json");
        if (!File.Exists(path))
            return new PackageInfo { Name = fallbackName };

        try
        {
            return JsonSerializer.Deserialize<PackageInfo>(File.ReadAllText(path))
                   ?? new PackageInfo { Name = fallbackName };
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"[Cosmetics] Failed to parse cosmetics.json for '{fallbackName}': {e.Message}");
            return new PackageInfo { Name = fallbackName };
        }
    }
}
