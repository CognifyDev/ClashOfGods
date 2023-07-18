using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace COG.Utils;

public class ResourceUtils
{
    public static Dictionary<string, Sprite> CachedSprites = new();
    
    public static void WriteToFileFromResource(string toPath, string resourcePath)
    {
        if (File.Exists(toPath)) return;
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
        if (stream != null)
        {
            File.WriteAllBytes(toPath, stream.ReadFully());
        }
    }
    
    public static Sprite? LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            Texture2D texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            // ignored
        }

        return null;
    }
    public static Texture2D LoadTextureFromResources(string path)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
        var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        using MemoryStream ms = new();
        stream.CopyTo(ms);
        ImageConversion.LoadImage(texture, ms.ToArray(), false);
        return texture;
    }
}