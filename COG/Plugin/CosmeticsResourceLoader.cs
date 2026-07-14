using System;
using System.IO;
using System.Linq;
using COG.Cosmetics;
using COG.Cosmetics.Hats;
using COG.Cosmetics.Nameplates;
using COG.Cosmetics.Visors;

namespace COG.Plugin;

public static class CosmeticsResourceLoader
{
    public static bool LoadCosmetics(LoadedResource resource)
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

            var hatLoader = new HatLoader();
            var visorLoader = new VisorLoader();
            var nameplateLoader = new NameplateLoader();

            int count = 0;

            var hasHats = resource.Archive.Entries.Any(e =>
                !string.IsNullOrEmpty(e.Name) &&
                LoadedResource.NormalizePath(e.FullName).StartsWith("Hats/", StringComparison.OrdinalIgnoreCase));

            var hasVisors = resource.Archive.Entries.Any(e =>
                !string.IsNullOrEmpty(e.Name) &&
                (LoadedResource.NormalizePath(e.FullName).StartsWith("Visors/", StringComparison.OrdinalIgnoreCase) ||
                 LoadedResource.NormalizePath(e.FullName).StartsWith("Visions/", StringComparison.OrdinalIgnoreCase)));

            var hasNameplates = resource.Archive.Entries.Any(e =>
                !string.IsNullOrEmpty(e.Name) &&
                (LoadedResource.NormalizePath(e.FullName).StartsWith("NamePlates/", StringComparison.OrdinalIgnoreCase) ||
                 LoadedResource.NormalizePath(e.FullName).StartsWith("Nameplates/", StringComparison.OrdinalIgnoreCase)));

            if (!hasHats && !hasVisors && !hasNameplates)
            {
                resource.Status = ResourceLoadStatus.Failed;
                resource.ErrorMessage = "No Hats/, Visors/, or NamePlates/ directories found";
                return false;
            }

            if (hasHats) LoadSection(hatLoader, resource);
            if (hasVisors) LoadSection(visorLoader, resource);
            if (hasNameplates) LoadSection(nameplateLoader, resource);

            count = hatLoader.CustomHats.Count + visorLoader.CustomVisors.Count + nameplateLoader.CustomNamePlates.Count;

            var cm = CosmeticsManager.Instance;
            cm.MergeHatLoader(hatLoader);
            cm.MergeVisorLoader(visorLoader);
            cm.MergeNameplateLoader(nameplateLoader);

            resource.CosmeticsCount = count;
            resource.Status = ResourceLoadStatus.Loaded;
            Main.Logger.LogInfo($"[CosmeticsResource] {resource.ResourceId}: {count} cosmetic(s) loaded");
            return true;
        }
        catch (System.Exception ex)
        {
            resource.Status = ResourceLoadStatus.Failed;
            resource.ErrorMessage = ex.Message;
            Main.Logger.LogError($"[CosmeticsResource] {resource.ResourceId} failed: {ex}");
            return false;
        }
    }

    private static void LoadSection(BaseLoader loader, LoadedResource resource)
    {
        var cacheDir = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"cog_cosmetic_{resource.ResourceId}_{Guid.NewGuid():N}"
        );

        try
        {
            System.IO.Directory.CreateDirectory(cacheDir);

            foreach (var entry in resource.Archive!.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name)) continue;

                var full = LoadedResource.NormalizePath(entry.FullName);
                var targetDir = GetTargetDir(full);
                if (targetDir == null) continue;

                var typeCacheDir = System.IO.Path.Combine(cacheDir, targetDir);
                System.IO.Directory.CreateDirectory(typeCacheDir);

                var destPath = System.IO.Path.Combine(typeCacheDir, entry.Name);
                if (!destPath.StartsWith(cacheDir)) continue;

                try
                {
                    using var entryStream = entry.Open();
                    using var fileStream = File.Create(destPath);
                    entryStream.CopyTo(fileStream);
                }
                catch { }
            }

            loader.LoadCosmetics(cacheDir, resource.Meta.Author);
        }
        finally
        {
            try { System.IO.Directory.Delete(cacheDir, true); } catch { }
        }
    }

    private static string GetTargetDir(string normalizedPath)
    {
        if (normalizedPath.StartsWith("Hats/", StringComparison.OrdinalIgnoreCase)) return "Hats";
        if (normalizedPath.StartsWith("Visors/", StringComparison.OrdinalIgnoreCase) ||
            normalizedPath.StartsWith("Visions/", StringComparison.OrdinalIgnoreCase)) return "Visors";
        if (normalizedPath.StartsWith("NamePlates/", StringComparison.OrdinalIgnoreCase) ||
            normalizedPath.StartsWith("Nameplates/", StringComparison.OrdinalIgnoreCase)) return "NamePlates";
        return null!;
    }

}
