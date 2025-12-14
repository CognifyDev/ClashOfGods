using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using COG.Config;
using UnityEngine;

namespace COG.Utils;

public static class ResourceUtils
{
    private const string FileListURL =
        "https://download.hayashiume.top/https://raw.githubusercontent.com/CognifyDev/ClashOfGods/refs/heads/main/Resources/resources.txt";
    private const string TargetURL =
        "https://download.hayashiume.top/https://raw.githubusercontent.com/CognifyDev/ClashOfGods/refs/heads/main/Resources/";
    
    private static readonly Dictionary<string, byte[]> Cache = new();
    private static readonly Dictionary<string, Sprite> CachedSprites = new();

    private const string CacheDataDir = ".\\" + ConfigBase.DataDirectoryName + "\\cache";

    public static bool ContainsResource(string path)
    {
        return Cache.ContainsKey(path);
    }
    
    public static Sprite LoadSprite(string path, float pixelsPerUnit = 100f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            try
            {
                var texture = LoadTextureFromResources(path);
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f),
                    pixelsPerUnit);
                sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
                return CachedSprites[path + pixelsPerUnit] = sprite;
            }
            catch (System.Exception e)
            {
                Main.Logger.LogError($"Failed to load sprite: {e}");
            }
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"Error while loading {path} ({pixelsPerUnit}): {e}");
            throw new System.Exception($"Failed to load {path}");
        }

        throw new System.Exception("Failed to load sprite");
    }

    private static Texture2D LoadTextureFromResources(string path)
    {
        var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        var succeed = texture.LoadImage(DownloadFromURLOrGetFromCache(path), false);
        if (!succeed) Main.Logger.LogError("Failed to load texture: " + path);
        return texture;
    }

    static ResourceUtils()
    {
        if (!Directory.Exists(CacheDataDir))
        {
            Directory.CreateDirectory(CacheDataDir);
        }
        
        foreach (var filePath in Directory.GetFiles(CacheDataDir, "*.*", SearchOption.AllDirectories))
        {
            // .\ClashOfGods_DATA\cache\filename
            var path = filePath.Replace(CacheDataDir + "\\", "").Replace("\\", "/");
            var bytes = File.ReadAllBytes(filePath);
            Cache.Add(path, bytes);
        }

        var fileListBytes = DownloadFromURLOrGetFromCache(FileListURL, true);
        if (fileListBytes.Length > 0)
        {
            var fileList = Encoding.UTF8.GetString(fileListBytes);
            foreach (var currentFile in fileList.Split("\n"))
            {
                if (!currentFile.Contains(',')) continue;
                var split = currentFile.Split(",");
                var filePath = split[0];
                var sha1 = split[0];

                var target = CacheDataDir + "\\" + filePath;
                if (File.Exists(target))
                {
                    if (sha1.Equals(GetFileSHA1(target))) continue;
                    File.Delete(target);
                    File.WriteAllBytes(target, DownloadFromURLOrGetFromCache(filePath));
                }
                else
                {
                    var directoryName = Path.GetDirectoryName(target);
                    if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName)) 
                        Directory.CreateDirectory(directoryName);
                    File.WriteAllBytes(target, DownloadFromURLOrGetFromCache(filePath));
                }
            }
        }
        else
        {
            Main.Logger.LogError("Failed to update cache");
            Main.Logger.LogError("Please reboot the game and then try again");
        }
    }
    
    private static string GetFileSHA1(string filePath)
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