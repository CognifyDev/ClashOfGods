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

namespace COG.Cosmetics.Nameplates;

public sealed class NameplateLoader : BaseLoader
{
    public Dictionary<string, CustomNamePlate> CustomNamePlates { get; } = [];

    public override string GetCosmeticId(string name) => $"cog.cosmetic.nameplate.{name}";

    public override void InstallCosmetics(ReferenceData refData)
    {
        foreach (var (id, np) in CustomNamePlates)
        {
            try
            {
                refData.nameplates.Add(np.NamePlateData);
                Main.Logger.LogDebug($"[NameplateLoader] Installed nameplate '{id}'.");
            }
            catch (System.Exception e)
            {
                Main.Logger.LogError($"[NameplateLoader] Failed to install nameplate '{id}':\n{e}");
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
                if (LoadNamePlate(file, author))
                    Main.Logger.LogInfo($"[NameplateLoader] Loaded: {Path.GetFileName(file)}");
                else
                    Main.Logger.LogError($"[NameplateLoader] Failed: {Path.GetFileName(file)}");
            }
            catch (System.Exception e)
            {
                Main.Logger.LogError($"[NameplateLoader] Exception loading '{file}': {e.Message}");
            }
        }
    }

    public override bool LocateCosmetic(
        string id,
        string type,
        [NotNullWhen(true)] out Il2CppSystem.Type? il2CppType)
    {
        il2CppType = null;
        if (!CustomNamePlates.ContainsKey(id)) return false;
        il2CppType = type == ReferenceType.NamePlateViewData ? Il2CppType.Of<NamePlateViewData>() : null;
        return il2CppType != null;
    }

    public override bool ProvideCosmetic(ProvideHandle handle, string id, string type)
    {
        if (!CustomNamePlates.TryGetValue(id, out var np)) return false;

        switch (type)
        {
            case ReferenceType.Preview:
                handle.Complete(np.PreviewData, true, null);
                return true;
            case ReferenceType.NamePlateViewData:
                handle.Complete(np.NamePlateViewData, true, null);
                return true;
            default:
                Main.Logger.LogWarning($"[NameplateLoader] Unknown type token '{type}' for nameplate '{id}'.");
                return false;
        }
    }

    private bool LoadNamePlate(string filePath, string author)
    {
        var stem        = Path.GetFileNameWithoutExtension(filePath);
        var displayName = string.IsNullOrWhiteSpace(author) ? stem : $"{stem} by {author}";
        var fullId      = GetCosmeticId(stem);

        var sprite = SpriteUtils.LoadSpriteFromFile(filePath);
        if (sprite == null)
        {
            Main.Logger.LogError($"[NameplateLoader] Sprite load failed for '{stem}'.");
            return false;
        }
        sprite.MarkDontUnload();

        var viewData = ScriptableObject.CreateInstance<NamePlateViewData>();
        viewData.name  = displayName;
        viewData.Image = sprite;

        var previewData = ScriptableObject.CreateInstance<PreviewViewData>();
        previewData.name          = displayName;
        previewData.PreviewSprite = sprite;

        var npData         = ScriptableObject.CreateInstance<NamePlateData>();
        npData.name        = displayName;
        npData.Free        = true;
        npData.ProductId   = fullId;
        npData.ViewDataRef = new AssetReference(CogHatLocator.GetGuid(fullId, ReferenceType.NamePlateViewData));
        npData.PreviewData = new AssetReference(CogHatLocator.GetGuid(fullId, ReferenceType.Preview));

        CustomNamePlates[fullId] = new CustomNamePlate(fullId, npData, viewData, previewData);

        npData.ViewDataRef.LoadAsset<NamePlateViewData>();
        npData.PreviewData.LoadAsset<PreviewViewData>();
        return true;
    }
}
