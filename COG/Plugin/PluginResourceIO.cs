using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace COG.Plugin;

public static class PluginContext
{
    public static IPluginResourceIO Current { get; internal set; } = null!;
}

public interface IPluginResourceIO
{
    string GetString(string key);
    Sprite GetSprite(string path, float pixelsPerUnit = 100f);
    byte[] GetResourceBytes(string path);
    string GetResourceText(string path);
    bool ResourceExists(string path);
}

internal sealed class PluginResourceIO : IPluginResourceIO
{
    private readonly Dictionary<string, string> _languages = new();
    private readonly Dictionary<string, byte[]> _resources = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _pluginName;

    private static readonly Dictionary<string, LoadedResource> ResourceMap = new();

    internal PluginResourceIO(string pluginName, ZipArchive archive)
    {
        _pluginName = pluginName;
        SafeLoadLanguages(archive);
        SafeLoadResources(archive);
    }

    internal static void RegisterResource(LoadedResource resource)
    {
        ResourceMap[resource.ResourceId] = resource;
    }

    internal static void UnregisterResource(string resourceId)
    {
        ResourceMap.Remove(resourceId);
    }

    public static byte[] ReadResourceBytes(string address)
    {
        try
        {
            var parts = address.Split("::", 2);
            if (parts.Length != 2) return null!;
            if (!ResourceMap.TryGetValue(parts[0], out var resource)) return null!;
            return resource.ReadZipEntry($"Resources/{parts[1]}");
        }
        catch
        {
            return null!;
        }
    }

    public static string ReadResourceText(string address)
    {
        var bytes = ReadResourceBytes(address);
        return bytes != null ? Encoding.UTF8.GetString(bytes) : "";
    }

    public static Sprite LoadSprite(string resourceId, string innerPath, float pixelsPerUnit = 100f)
    {
        try
        {
            var bytes = ReadResourceBytes($"{resourceId}::{innerPath}");
            if (bytes == null || bytes.Length == 0) return null!;

            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            if (!texture.LoadImage(bytes, false)) return null!;

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return sprite;
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"[PluginResourceIO] Failed to load sprite '{resourceId}::{innerPath}': {ex.Message}");
            return null!;
        }
    }

    public static bool ResourceExists(string resourceId, string innerPath)
    {
        try
        {
            if (!ResourceMap.TryGetValue(resourceId, out var resource)) return false;
            return resource.ZipEntryExists($"Resources/{innerPath}");
        }
        catch
        {
            return false;
        }
    }

    private void SafeLoadLanguages(ZipArchive archive)
    {
        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name)) continue;
            var full = LoadedResource.NormalizePath(entry.FullName);
            if (!full.StartsWith("Languages/", StringComparison.OrdinalIgnoreCase)) continue;
            if (!full.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) &&
                !full.EndsWith(".yml", StringComparison.OrdinalIgnoreCase)) continue;

            try
            {
                using var stream = entry.Open();
                using var reader = new StreamReader(stream);
                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                if (yamlStream.Documents.Count == 0 || yamlStream.Documents[0].RootNode == null) continue;

                var root = (YamlMappingNode)yamlStream.Documents[0].RootNode;
                FlattenMapping(root, "", _languages);
            }
            catch (System.Exception ex)
            {
                Main.Logger.LogError($"[PluginResourceIO:{_pluginName}] Failed to parse language '{entry.Name}': {ex.Message}");
            }
        }
    }

    private void SafeLoadResources(ZipArchive archive)
    {
        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name)) continue;
            var full = LoadedResource.NormalizePath(entry.FullName);
            if (!full.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase)) continue;

            try
            {
                using var stream = entry.Open();
                using var ms = new MemoryStream();
                stream.CopyTo(ms);
                var relativePath = full["Resources/".Length..];
                _resources[relativePath] = ms.ToArray();
            }
            catch (System.Exception ex)
            {
                Main.Logger.LogError($"[PluginResourceIO:{_pluginName}] Failed to load resource '{entry.Name}': {ex.Message}");
            }
        }
    }

    private static void FlattenMapping(YamlMappingNode node, string prefix, Dictionary<string, string> target)
    {
        foreach (var kvp in node.Children)
        {
            var key = ((YamlScalarNode)kvp.Key).Value ?? "";
            var fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";

            if (kvp.Value is YamlMappingNode childMapping)
            {
                FlattenMapping(childMapping, fullKey, target);
            }
            else if (kvp.Value is YamlScalarNode scalarNode)
            {
                target[fullKey] = scalarNode.Value ?? "";
            }
        }
    }

    public string GetString(string key)
    {
        if (string.IsNullOrEmpty(key)) return "";
        if (_languages.TryGetValue(key, out var value)) return value;
        Main.Logger.LogDebug($"[PluginResourceIO:{_pluginName}] Missing language key: {key}");
        return key;
    }

    public Sprite GetSprite(string path, float pixelsPerUnit = 100f)
    {
        if (string.IsNullOrEmpty(path)) return null!;

        try
        {
            var bytes = GetResourceBytes(path);
            if (bytes == null || bytes.Length == 0) return null!;

            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            if (!texture.LoadImage(bytes, false)) return null!;

            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return sprite;
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"[PluginResourceIO:{_pluginName}] Failed to create sprite from '{path}': {ex.Message}");
            return null!;
        }
    }

    public byte[] GetResourceBytes(string path)
    {
        if (string.IsNullOrEmpty(path)) return [];

        if (_resources.TryGetValue(path, out var bytes)) return bytes;
        var normalizedPath = path.Replace('\\', '/');
        if (_resources.TryGetValue(normalizedPath, out bytes)) return bytes;

        Main.Logger.LogDebug($"[PluginResourceIO:{_pluginName}] Missing resource: {path}");
        return [];
    }

    public string GetResourceText(string path)
    {
        var bytes = GetResourceBytes(path);
        return bytes.Length > 0 ? Encoding.UTF8.GetString(bytes) : "";
    }

    public bool ResourceExists(string path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        return _resources.ContainsKey(path) || _resources.ContainsKey(path.Replace('\\', '/'));
    }
}
