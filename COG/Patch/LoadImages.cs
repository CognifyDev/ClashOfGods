using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace COG;
[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
public static class TitleLogo
{

    public static Dictionary<string, Sprite> CachedSprites = new();
    public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
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

    private static void Postfix(MainMenuManager __instance)
    {
        var CustomBG = new GameObject("CustomBG");
        CustomBG.transform.position = new Vector3(2, 0f, 0);
        var bgRenderer = CustomBG.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = LoadSprite("COG.Resources.InDLL.Images.COG-BG.png", 280f);
    }
}


