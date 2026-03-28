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

namespace COG.Cosmetics.Visors;

public sealed class VisorLoader : BaseLoader
{
    public Dictionary<string, CustomVisor> CustomVisors { get; } = [];

    public override string GetCosmeticId(string name) => $"cog.cosmetic.visor.{name}";

    public override void InstallCosmetics(ReferenceData refData)
    {
        foreach (var (id, visor) in CustomVisors)
        {
            try
            {
                refData.visors.Add(visor.VisorData);
                Main.Logger.LogDebug($"[VisorLoader] Installed visor '{id}'.");
            }
            catch (System.Exception e)
            {
                Main.Logger.LogError($"[VisorLoader] Failed to install visor '{id}':\n{e}");
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
                if (LoadVisor(file, author))
                    Main.Logger.LogInfo($"[VisorLoader] Loaded: {Path.GetFileName(file)}");
                else
                    Main.Logger.LogError($"[VisorLoader] Failed: {Path.GetFileName(file)}");
            }
            catch (System.Exception e)
            {
                Main.Logger.LogError($"[VisorLoader] Exception loading '{file}': {e.Message}");
            }
        }
    }

    public override bool LocateCosmetic(
        string id,
        string type,
        [NotNullWhen(true)] out Il2CppSystem.Type? il2CppType)
    {
        il2CppType = null;
        if (!CustomVisors.ContainsKey(id)) return false;
        il2CppType = type == ReferenceType.VisorViewData ? Il2CppType.Of<VisorViewData>() : null;
        return il2CppType != null;
    }

    public override bool ProvideCosmetic(ProvideHandle handle, string id, string type)
    {
        if (!CustomVisors.TryGetValue(id, out var visor)) return false;

        switch (type)
        {
            case ReferenceType.Preview:
                handle.Complete(visor.PreviewData, true, null);
                return true;
            case ReferenceType.VisorViewData:
                handle.Complete(visor.VisorViewData, true, null);
                return true;
            default:
                Main.Logger.LogWarning($"[VisorLoader] Unknown type token '{type}' for visor '{id}'.");
                return false;
        }
    }

    private bool LoadVisor(string filePath, string author)
    {
        var stem        = Path.GetFileNameWithoutExtension(filePath);
        var displayName = string.IsNullOrWhiteSpace(author) ? stem : $"{stem} by {author}";
        var fullId      = GetCosmeticId(stem);

        var sprite = SpriteUtils.LoadSpriteFromFile(filePath);
        if (sprite == null)
        {
            Main.Logger.LogError($"[VisorLoader] Sprite load failed for '{stem}'.");
            return false;
        }
        sprite.MarkDontUnload();

        var viewData = ScriptableObject.CreateInstance<VisorViewData>();
        viewData.name       = displayName;
        viewData.ClimbFrame = SpriteUtils.EmptySprite;
        viewData.IdleFrame
            = viewData.LeftIdleFrame
            = viewData.FloorFrame
            = sprite;

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name          = displayName;
        previewData.PreviewSprite = sprite;

        var visorData         = ScriptableObject.CreateInstance<VisorData>();
        visorData.name        = displayName;
        visorData.Free        = true;
        visorData.ProductId   = fullId;
        visorData.ViewDataRef = new AssetReference(CogHatLocator.GetGuid(fullId, ReferenceType.VisorViewData));
        visorData.PreviewData = new AssetReference(CogHatLocator.GetGuid(fullId, ReferenceType.Preview));

        CustomVisors[fullId] = new CustomVisor(fullId, visorData, viewData, previewData);

        visorData.ViewDataRef.LoadAsset<VisorViewData>();
        visorData.PreviewData.LoadAsset<PreviewViewData>();
        return true;
    }
}
