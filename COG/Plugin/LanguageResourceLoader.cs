using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using COG.Config.Impl;
using YamlDotNet.RepresentationModel;

namespace COG.Plugin;

public static class LanguageResourceLoader
{
    private static readonly Dictionary<string, Dictionary<string, HashSet<string>>> LoadedKeys = new();

    public static bool LoadLanguage(LoadedResource resource)
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

            var yamlEntries = resource.Archive.Entries
                .Where(e => !string.IsNullOrEmpty(e.Name)
                            && LoadedResource.NormalizePath(e.FullName).StartsWith("Languages/", StringComparison.OrdinalIgnoreCase)
                            && e.FullName.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (yamlEntries.Count == 0)
            {
                resource.Status = ResourceLoadStatus.Failed;
                resource.ErrorMessage = "No Languages/*.yaml files found";
                return false;
            }

            var langIds = new List<string>();
            var totalEntries = 0;
            var resourceKeyMap = new Dictionary<string, HashSet<string>>();

            foreach (var entry in yamlEntries)
            {
                try
                {
                    using var stream = entry.Open();
                    using var reader = new StreamReader(stream);
                    var yamlStream = new YamlStream();
                    yamlStream.Load(reader);

                    if (yamlStream.Documents.Count == 0 || yamlStream.Documents[0].RootNode == null)
                    {
                        Main.Logger.LogWarning($"[LanguageResource] {entry.Name} is empty or invalid");
                        continue;
                    }

                    var mapping = (YamlMappingNode)yamlStream.Documents[0].RootNode;
                    string langId = null!;
                    var entries = new Dictionary<string, string>();

                    foreach (var kvp in mapping.Children)
                    {
                        var key = ((YamlScalarNode)kvp.Key).Value ?? "";
                        var value = ((YamlScalarNode)kvp.Value).Value ?? "";

                        if (key == "LangID")
                        {
                            langId = value;
                            continue;
                        }

                        entries[key] = value;
                    }

                    if (string.IsNullOrEmpty(langId))
                    {
                        Main.Logger.LogWarning($"[LanguageResource] {entry.Name} missing LangID");
                        continue;
                    }

                    LanguageConfig.AddCustomTranslation(langId, entries);

                    if (!resourceKeyMap.ContainsKey(langId))
                        resourceKeyMap[langId] = new HashSet<string>();

                    foreach (var key in entries.Keys)
                        resourceKeyMap[langId].Add(key);

                    langIds.Add($"{entry.Name}(ID:{langId})");
                    totalEntries += entries.Count;
                }
                catch (System.Exception ex)
                {
                    Main.Logger.LogError($"[LanguageResource] Failed to parse '{entry.Name}': {ex.Message}");
                }
            }

            if (langIds.Count == 0)
            {
                resource.Status = ResourceLoadStatus.Failed;
                resource.ErrorMessage = "No valid language files found";
                return false;
            }

            LoadedKeys[resource.ResourceId] = resourceKeyMap;
            resource.LanguageIds = langIds;
            resource.Status = ResourceLoadStatus.Loaded;
            Main.Logger.LogInfo($"[LanguageResource] {resource.ResourceId}: {langIds.Count} file(s), {totalEntries} entries");
            return true;
        }
        catch (System.Exception ex)
        {
            resource.Status = ResourceLoadStatus.Failed;
            resource.ErrorMessage = ex.Message;
            Main.Logger.LogError($"[LanguageResource] {resource.ResourceId} failed: {ex}");
            return false;
        }
    }

    public static void UnloadLanguage(string resourceId)
    {
        if (!LoadedKeys.TryGetValue(resourceId, out var langKeys))
            return;

        foreach (var (langId, keys) in langKeys)
        {
            LanguageConfig.RemoveCustomTranslation(langId, keys);
        }

        LoadedKeys.Remove(resourceId);
        Main.Logger.LogInfo($"[LanguageResource] {resourceId} unloaded");
    }
}
