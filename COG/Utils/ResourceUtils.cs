using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using COG.Config;
using UnityEngine;

namespace COG.Utils;

public static class ResourceUtils
{
    public const string FileListURL =
        $"{TargetURL}resources.txt";
    public const string TargetURL =
        "https://download.hayashiume.top/https://raw.githubusercontent.com/CognifyDev/ClashOfGods/refs/heads/main/Resources/";
    
    private static readonly Dictionary<string, byte[]> Cache = new();
    private static readonly Dictionary<string, Sprite> CachedSprites = new();

    public const string CacheDataDir = "./" + ConfigBase.DataDirectoryName + "/cache";

    public static bool ContainsResource(string path)
    {
        return Cache.ContainsKey(path);
    }

    public static Sprite LoadSpriteFromResources(string path, float pixelsPerUnit = 100f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite))
                return sprite;
            
            var texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 
                new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"Error while loading {path} ({pixelsPerUnit}): {e}");
            return null;
        }
    }

    public static Sprite LoadSprite(string path, float pixelsPerUnit = 100f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite))
                return sprite;
            
            var texture = LoadTextureFromDisk(path);
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 
                new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"Error while loading {path} ({pixelsPerUnit}): {e}");
            throw new System.Exception($"Failed to load {path}");
        }
    }

    private static Texture2D LoadTextureFromResources(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        using MemoryStream ms = new();
        stream?.CopyTo(ms);
        var succeed = texture.LoadImage(ms.ToArray(), false);
        if (!succeed) Main.Logger.LogError("Failed to load texture: " + path);
        return texture;
    }

    private static Texture2D LoadTextureFromDisk(string path)
    {
        var fullPath = Path.Combine(CacheDataDir, path);
        if (!File.Exists(fullPath))
        {
            Main.Logger.LogError($"File not found: {path}");
            return new Texture2D(1, 1);
        }
        
        var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        var fileBytes = File.ReadAllBytes(fullPath);
        var succeed = texture.LoadImage(fileBytes, false);
        if (!succeed) Main.Logger.LogError("Failed to load texture: " + path);
        return texture;
    }
    
    public static byte[] GetResourceBytes(string path)
    {
        if (Cache.TryGetValue(path, out var bytes))
            return bytes;
        return DownloadFromURLOrGetFromCache(path, false);
    }
    
    /// <summary>
    /// 直接从完整 URL 下载数据，不拼接 TargetURL 前缀
    /// </summary>
    public static byte[] DownloadFromURL(string url)
    {
        return DownloadFromURLOrGetFromCache(url, true);
    }
    
    /// <summary>
    /// 异步从完整 URL 下载数据
    /// </summary>
    public static async Task<byte[]> DownloadFileAsync(string url)
    {
        using var httpClient = new HttpClient();
        Main.Logger.LogInfo("Downloading file from: " + url);
        try
        {
            return await httpClient.GetByteArrayAsync(url);
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"Failed to download {url}: {e.Message}");
            return [];
        }
    }
    
    /// <summary>
    /// 将数据保存到缓存目录，并同步更新内存缓存
    /// </summary>
    public static void SaveToCache(string relativePath, byte[] data)
    {
        var localPath = Path.Combine(CacheDataDir, relativePath);
        var dir = Path.GetDirectoryName(localPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllBytes(localPath, data);
        if (Cache.ContainsKey(relativePath))
            Cache[relativePath] = data;
        else
            Cache.Add(relativePath, data);
    }
    static ResourceUtils()
    {
        if (!Directory.Exists(CacheDataDir))
        {
            Directory.CreateDirectory(CacheDataDir);
        }
        
        foreach (var filePath in Directory.GetFiles(CacheDataDir, "*.*", SearchOption.AllDirectories))
        {
            var path = filePath.Replace(CacheDataDir + "\\", "").Replace("\\", "/");
            var bytes = File.ReadAllBytes(filePath);
            Cache.Add(path, bytes);
        }
    }
    public static string GetFileSHA1(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Cannot find file：{filePath}");

        using var sha1 = SHA1.Create();
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var hashBytes = sha1.ComputeHash(fs);

        var sb = new StringBuilder();
        foreach (var b in hashBytes)
        {
            sb.Append(b.ToString("x2")); // "x2" 表示以小写十六进制格式输出，两位对齐
        }
        return sb.ToString();
    }
    private static byte[] DownloadFromURLOrGetFromCache(string path, bool isURL = false)
    {
        if (!isURL)
            if (Cache.TryGetValue(path, out var bytes))
                return bytes;
        
        var targetURL = isURL ? path : $"{TargetURL}{path}";
        
        var result = Task.Run(async () =>
        {
            using var httpClient = new HttpClient();
            Main.Logger.LogInfo("Downloading needed file from: " + targetURL);
            try
            {
                return await httpClient.GetByteArrayAsync(targetURL);
            }
            catch
            {
                Main.Logger.LogError("Failed to download target file.");
                return [];
            }
        }).Result;
        
        if (!isURL) Cache.Add(path, result);
        return result;
    }
}