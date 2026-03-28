using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using COG.Cosmetics.Unity;
using COG.Utils;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace COG.Cosmetics.Hats;


public sealed class HatLoader : BaseLoader
{
    public Dictionary<string, CustomHat> CustomHats { get; } = [];

    public override string GetCosmeticId(string name) => $"cog.cosmetic.hat.{name}";

    public override void InstallCosmetics(ReferenceData refData)
    {
        foreach (var (id, hat) in CustomHats)
        {
            try
            {
                refData.hats.Add(hat.HatData);
                Main.Logger.LogDebug($"[HatLoader] Installed hat '{id}'.");
            }
            catch (System.Exception e)
            {
                Main.Logger.LogError($"[HatLoader] Failed to install hat '{id}':\n{e}");
            }
        }
    }

    public override void LoadCosmetics(string directory, string author = "")
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            return;
        }

        foreach (var file in Directory.GetFiles(directory, "*.png"))
        {
            try
            {
                if (LoadHat(file, author))
                    Main.Logger.LogInfo($"[HatLoader] Loaded: {Path.GetFileName(file)}");
                else
                    Main.Logger.LogError($"[HatLoader] Failed: {Path.GetFileName(file)}");
            }
            catch (System.Exception e)
            {
                Main.Logger.LogError($"[HatLoader] Exception loading '{file}': {e.Message}");
            }
        }
    }

    public override bool LocateCosmetic(
        string id,
        string type,
        [NotNullWhen(true)] out Il2CppSystem.Type? il2CppType)
    {
        il2CppType = null;
        if (!CustomHats.ContainsKey(id)) return false;
        il2CppType = type == ReferenceType.HatViewData ? Il2CppType.Of<HatViewData>() : null;
        return il2CppType != null;
    }

    public override bool ProvideCosmetic(ProvideHandle handle, string id, string type)
    {
        if (!CustomHats.TryGetValue(id, out var hat)) return false;

        switch (type)
        {
            case ReferenceType.Preview:
                handle.Complete(hat.PreviewData, true, null);
                return true;
            case ReferenceType.HatViewData:
                handle.Complete(hat.HatViewData, true, null);
                return true;
            default:
                Main.Logger.LogWarning($"[HatLoader] Unknown type token '{type}' for hat '{id}'.");
                return false;
        }
    }

    private bool LoadHat(string filePath, string author)
    {
        var stem        = Path.GetFileNameWithoutExtension(filePath);
        var displayName = string.IsNullOrWhiteSpace(author) ? stem : $"{stem} by {author}";
        var fullId      = GetCosmeticId(stem);

        var hatSprite = SpriteUtils.LoadSpriteFromFile(filePath);
        if (hatSprite == null)
        {
            Main.Logger.LogError($"[HatLoader] Sprite load failed for '{stem}'.");
            return false;
        }
        hatSprite.MarkDontUnload();

        var viewData = ScriptableObject.CreateInstance<HatViewData>();
        viewData.name      = displayName;
        viewData.MainImage = hatSprite;
        
        TrySetOptional(filePath, "climb",     s => viewData.ClimbImage     = s);
        TrySetOptional(filePath, "floor",     s => viewData.FloorImage     = s);
        TrySetOptional(filePath, "back",      s => viewData.BackImage      = s);
        TrySetOptional(filePath, "left",      s => viewData.LeftMainImage  = s);
        TrySetOptional(filePath, "leftback",  s => viewData.LeftBackImage  = s);
        TrySetOptional(filePath, "leftclimb", s => viewData.LeftClimbImage = s);
        TrySetOptional(filePath, "leftfloor", s => viewData.LeftFloorImage = s);

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name          = displayName;
        previewData.PreviewSprite = hatSprite;

        var hatData             = ScriptableObject.CreateInstance<HatData>();
        hatData.name            = hatData.StoreName = displayName;
        hatData.Free            = true;
        hatData.ProductId       = fullId;
        hatData.InFront         = true;
        hatData.NoBounce        = true;
        hatData.ViewDataRef     = new AssetReference(CogHatLocator.GetGuid(fullId, ReferenceType.HatViewData));
        hatData.PreviewData     = new AssetReference(CogHatLocator.GetGuid(fullId, ReferenceType.Preview));

        CustomHats[fullId] = new CustomHat(fullId, hatData, viewData, previewData);

        hatData.ViewDataRef.LoadAsset<HatViewData>();
        hatData.PreviewData.LoadAsset<PreviewViewData>();
        return true;
    }

    private static void TrySetOptional(string basePath, string suffix, Action<Sprite> assign)
    {
        var dir      = Path.GetDirectoryName(basePath)!;
        var stem     = Path.GetFileNameWithoutExtension(basePath);
        var candidate = Path.Combine(dir, $"{stem}.{suffix}.png");

        var sprite = SpriteUtils.LoadSpriteFromFile(candidate);
        if (sprite == null) return;
        sprite.MarkDontUnload();
        assign(sprite);
    }
}
