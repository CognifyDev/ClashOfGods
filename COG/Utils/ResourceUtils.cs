using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace COG.Utils;

public static class ResourceUtils
{
    private static readonly Dictionary<string, Sprite> CachedSprites = new();

    public static void WriteToFileFromResource(string toPath, string resourcePath)
    {
        if (File.Exists(toPath)) return;
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
        if (stream != null) File.WriteAllBytes(toPath, stream.ReadFully());
    }

    public static Texture2D? LoadTextureFromDisk(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                var byteTexture = Il2CppSystem.IO.File.ReadAllBytes(path);
                texture.LoadImage(byteTexture, false);
                return texture;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    public static Sprite? LoadSprite(string path, float pixelsPerUnit = 100f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            var texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"Error while loading {path} ({pixelsPerUnit}): {e}");
            return null;
        }
    }

    public static Texture2D LoadTextureFromResources(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        using MemoryStream ms = new();
        stream?.CopyTo(ms);
        var succeed = texture.LoadImage(ms.ToArray(), false);
        if (!succeed) Main.Logger.LogError("Failed to load texture: " + path);
        return texture;
    }

    public static byte[] ReadFully(this Stream input)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }

    public static string GetResourcePath(string pathToFile)
    {
        return $"COG.Resources.{pathToFile}";
    }
}