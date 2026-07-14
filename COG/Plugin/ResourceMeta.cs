using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace COG.Plugin;

public enum ResourceType
{
    Plugin,
    Language,
    Cosmetics,
    SpriteReplace
}

public class ResourceMeta
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = "Unknown";

    [JsonPropertyName("Author")]
    public string Author { get; set; } = "Unknown";

    [JsonPropertyName("Description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("Version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("Type")]
    public string Type { get; set; } = "Plugin";

    [JsonPropertyName("Id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("Dependency")]
    public List<string> Dependency { get; set; } = [];

    [JsonPropertyName("Hidden")]
    public bool Hidden { get; set; }

    public ResourceType GetResourceType() => Type switch
    {
        "Plugin" => ResourceType.Plugin,
        "Language" => ResourceType.Language,
        "Cosmetics" => ResourceType.Cosmetics,
        "SpriteReplace" => ResourceType.SpriteReplace,
        _ => ResourceType.Plugin
    };
}

public enum ResourceLoadStatus
{
    Discovered,
    Loading,
    Loaded,
    Failed
}

public class LoadedResource : IDisposable
{
    public ResourceMeta Meta { get; init; }
    public string FilePath { get; init; }
    public ResourceLoadStatus Status { get; set; } = ResourceLoadStatus.Discovered;
    public string ErrorMessage { get; set; } = "";
    public ZipArchive Archive { get; set; } = null!;

    public List<Assembly> LoadedAssemblies { get; set; } = [];
    public List<string> LanguageIds { get; set; } = [];
    public int SpriteReplaceCount { get; set; }
    public int CosmeticsCount { get; set; }

    public string ResourceId => string.IsNullOrEmpty(Meta.Id) ? Meta.Name : Meta.Id;
    public ResourceType ResourceType => Meta.GetResourceType();

    public LoadedResource(ResourceMeta meta, string filePath)
    {
        Meta = meta;
        FilePath = filePath;
    }

    public byte[] ReadZipEntry(string entryPath)
    {
        try
        {
            if (Archive == null) return null!;
            var entry = Archive.GetEntry(entryPath)
                        ?? Archive.Entries.FirstOrDefault(e =>
                            NormalizePath(e.FullName).EndsWith(entryPath, StringComparison.OrdinalIgnoreCase));
            if (entry == null) return null!;
            using var ms = new MemoryStream();
            using var stream = entry.Open();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
        catch (System.Exception)
        {
            return null!;
        }
    }

    public string ReadZipEntryAsString(string entryPath)
    {
        var bytes = ReadZipEntry(entryPath);
        return bytes != null ? System.Text.Encoding.UTF8.GetString(bytes) : "";
    }

    public Stream OpenZipEntryStream(string entryPath)
    {
        try
        {
            if (Archive == null) return null!;
            var entry = Archive.GetEntry(entryPath)
                        ?? Archive.Entries.FirstOrDefault(e =>
                            NormalizePath(e.FullName).EndsWith(entryPath, StringComparison.OrdinalIgnoreCase));
            return entry?.Open()!;
        }
        catch (System.Exception)
        {
            return null!;
        }
    }

    public bool ZipEntryExists(string entryPath)
    {
        try
        {
            if (Archive == null) return false;
            return Archive.GetEntry(entryPath) != null
                   || Archive.Entries.Any(e =>
                       NormalizePath(e.FullName).EndsWith(entryPath, StringComparison.OrdinalIgnoreCase));
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    public static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    public void Dispose()
    {
        try { Archive?.Dispose(); } catch { }
        Archive = null!;
    }
}
