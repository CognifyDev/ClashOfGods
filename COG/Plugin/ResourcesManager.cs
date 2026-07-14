using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using BepInEx;
using COG.Config;
using COG.Plugin.CSharp;
using UnityEngine;

namespace COG.Plugin;

public static class ResourcesManager
{
    public static readonly Dictionary<string, LoadedResource> AllResources = new();

    public static string ResourcesRootPath { get; } = OperatingSystem.IsAndroid()
        ? Path.Combine(Application.persistentDataPath, ConfigBase.DataDirectoryName, "Resources")
        : Path.Combine(Paths.GameRootPath, ConfigBase.DataDirectoryName, "Resources");

    public static bool LoadComplete { get; private set; }

    public static void CheckForResources()
    {
        LoadComplete = false;
        AllResources.Clear();

        Main.Logger.LogInfo("[ResourcesManager] Scanning for resources...");

        if (!Directory.Exists(ResourcesRootPath))
        {
            Directory.CreateDirectory(ResourcesRootPath);
            Main.Logger.LogInfo($"[ResourcesManager] Created: {ResourcesRootPath}");
            LoadComplete = true;
            return;
        }

        var zipFiles = Directory.GetFiles(ResourcesRootPath, "*.zip", SearchOption.TopDirectoryOnly);
        Main.Logger.LogInfo($"[ResourcesManager] Found {zipFiles.Length} .zip file(s)");

        if (zipFiles.Length == 0)
        {
            LoadComplete = true;
            return;
        }

        var discovered = new List<LoadedResource>();
        foreach (var zipPath in zipFiles)
        {
            var resource = DiscoverResource(zipPath);
            if (resource != null)
            {
                discovered.Add(resource);
                AllResources[resource.ResourceId] = resource;
            }
        }

        var ordered = discovered
            .OrderBy(r => r.ResourceType switch
            {
                ResourceType.Language => 0,
                ResourceType.SpriteReplace => 1,
                ResourceType.Cosmetics => 2,
                ResourceType.Plugin => 3,
                _ => 4
            })
            .ToList();

        foreach (var resource in ordered)
        {
            Main.Logger.LogInfo($"[ResourcesManager] Loading {resource.Meta.Name} ({resource.ResourceType})...");
            LoadResource(resource);
        }

        LoadComplete = true;

        var loaded = AllResources.Values.Count(r => r.Status == ResourceLoadStatus.Loaded);
        var failed = AllResources.Values.Count(r => r.Status == ResourceLoadStatus.Failed);
        Main.Logger.LogInfo($"[ResourcesManager] Done: {loaded} loaded, {failed} failed, {AllResources.Count} total");
    }

    private static LoadedResource DiscoverResource(string zipPath)
    {
        try
        {
            var archive = ZipFile.OpenRead(zipPath);
            var metaEntry = archive.GetEntry("metainfo.json");

            if (metaEntry == null)
            {
                archive.Dispose();
                Main.Logger.LogWarning($"[ResourcesManager] {Path.GetFileName(zipPath)}: missing metainfo.json, skipping");
                return null!;
            }

            using var stream = metaEntry.Open();
            var metaJson = new StreamReader(stream).ReadToEnd();
            var meta = JsonSerializer.Deserialize<ResourceMeta>(metaJson);

            if (meta == null)
            {
                archive.Dispose();
                Main.Logger.LogWarning($"[ResourcesManager] {Path.GetFileName(zipPath)}: failed to parse metainfo.json");
                return null!;
            }

            if (string.IsNullOrEmpty(meta.Id))
                meta.Id = Path.GetFileNameWithoutExtension(zipPath);

            var resource = new LoadedResource(meta, zipPath)
            {
                Archive = archive,
                Status = ResourceLoadStatus.Discovered
            };

            Main.Logger.LogInfo($"[ResourcesManager] Discovered: {meta.Name} ({resource.ResourceId}) type={meta.Type}");
            return resource;
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"[ResourcesManager] Failed to discover {Path.GetFileName(zipPath)}: {ex.Message}");
            return null!;
        }
    }

    private static void LoadResource(LoadedResource resource)
    {
        var success = resource.ResourceType switch
        {
            ResourceType.Language => LanguageResourceLoader.LoadLanguage(resource),
            ResourceType.Cosmetics => CosmeticsResourceLoader.LoadCosmetics(resource),
            ResourceType.Plugin => CSharpPluginLoader.LoadPlugin(resource),
            _ => false
        };

        if (!success)
        {
            Main.Logger.LogWarning($"[ResourcesManager] {resource.Meta.Name}: {resource.ErrorMessage}");
        }
    }

    public static void UnloadResource(string resourceId)
    {
        if (!AllResources.TryGetValue(resourceId, out var resource)) return;

        switch (resource.ResourceType)
        {
            case ResourceType.Language:
                LanguageResourceLoader.UnloadLanguage(resourceId);
                break;
            case ResourceType.Plugin:
                CSharpPluginLoader.UnloadPlugin(resourceId);
                break;
        }

        resource.Dispose();
        AllResources.Remove(resourceId);
        Main.Logger.LogInfo($"[ResourcesManager] Unloaded: {resourceId}");
    }
}
