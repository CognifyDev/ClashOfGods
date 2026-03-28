using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using COG.Cosmetics.Hats;
using COG.Cosmetics.Nameplates;
using COG.Cosmetics.Unity;
using COG.Cosmetics.Visors;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace COG.Cosmetics;

public sealed class CosmeticsManager
{
    private CosmeticsManager()
    {
        EmptyKeys     = new Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object>(_emptyKeyList.Pointer);
        CosmeticGroup = ScriptableObject.CreateInstance<CosmeticReleaseGroup>();
        CosmeticGroup.date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        _zipLoader = new ZipPackageLoader(_hatLoader, _visorLoader, _nameplateLoader);
    }

    private static CosmeticsManager? _instance;
    public  static CosmeticsManager   Instance => _instance ??= new CosmeticsManager();
    
    private readonly Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object> _emptyKeyList = new();
    public Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object> EmptyKeys { get; }
    
    public CosmeticReleaseGroup CosmeticGroup { get; }

    private readonly HatLoader       _hatLoader       = new();
    private readonly VisorLoader     _visorLoader     = new();
    private readonly NameplateLoader _nameplateLoader = new();
    private readonly ZipPackageLoader _zipLoader;
    
    private bool _loaded;
    
    public void LoadCosmetics()
    {
        if (_loaded) return;
        _loaded = true;

        Main.Logger.LogInfo("[Cosmetics] Scanning for zip packages…");
        _zipLoader.LoadAllPackages(CosmeticPaths.ZipPath, CosmeticPaths.CachePath);
        
        foreach (var id in _hatLoader.CustomHats.Keys)       CosmeticGroup.ids.Add(id);
        foreach (var id in _visorLoader.CustomVisors.Keys)   CosmeticGroup.ids.Add(id);
        foreach (var id in _nameplateLoader.CustomNamePlates.Keys) CosmeticGroup.ids.Add(id);

        var total = _hatLoader.CustomHats.Count
                  + _visorLoader.CustomVisors.Count
                  + _nameplateLoader.CustomNamePlates.Count;

        Main.Logger.LogInfo(
            $"[Cosmetics] Loaded {total} cosmetics " +
            $"({_hatLoader.CustomHats.Count} hats, " +
            $"{_visorLoader.CustomVisors.Count} visors, " +
            $"{_nameplateLoader.CustomNamePlates.Count} nameplates).");
    }
    
    public void InstallCosmetics(ReferenceData refData)
    {
        _hatLoader.InstallCosmetics(refData);
        _visorLoader.InstallCosmetics(refData);
        _nameplateLoader.InstallCosmetics(refData);

        var groups = refData.Groups.releaseGroups.ToList();
        groups.Add(CosmeticGroup);
        refData.Groups.releaseGroups = groups.ToArray();

        Main.Logger.LogInfo("[Cosmetics] Cosmetics installed into ReferenceData.");
    }
    
    public bool LocateCosmetic(
        string id,
        string type,
        [NotNullWhen(true)] out Il2CppSystem.Type? il2CppType)
    {
        il2CppType = null;
        try
        {
            if (type == ReferenceType.Preview)
            {
                il2CppType = Il2CppType.Of<PreviewViewData>();
                return true;
            }

            return _hatLoader.LocateCosmetic(id, type, out il2CppType)
                || _visorLoader.LocateCosmetic(id, type, out il2CppType)
                || _nameplateLoader.LocateCosmetic(id, type, out il2CppType);
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"[Cosmetics] LocateCosmetic error for '{id}':\n{e}");
            return false;
        }
    }
    
    public bool ProvideCosmetic(
        ProvideHandle handle,
        string id,
        string type,
        [NotNullWhen(false)] out System.Exception? exception)
    {
        exception = null;
        try
        {
            if (_hatLoader.ProvideCosmetic(handle, id, type))       return true;
            if (_visorLoader.ProvideCosmetic(handle, id, type))     return true;
            if (_nameplateLoader.ProvideCosmetic(handle, id, type)) return true;

            throw new InvalidOperationException($"No handler for '{id}' / '{type}'.");
        }
        catch (System.Exception e)
        {
            exception = e;
            return false;
        }
    }

    public bool TryGetHat      (string id, [NotNullWhen(true)] out CustomHat?       hat)       => _hatLoader.CustomHats.TryGetValue(id, out hat);
    public bool TryGetVisor    (string id, [NotNullWhen(true)] out CustomVisor?     visor)     => _visorLoader.CustomVisors.TryGetValue(id, out visor);
    public bool TryGetNamePlate(string id, [NotNullWhen(true)] out CustomNamePlate? namePlate) => _nameplateLoader.CustomNamePlates.TryGetValue(id, out namePlate);
    
    public (string[] Hats, string[] Visors, string[] NamePlates) GetLoadedIds() =>
    (
        [.. _hatLoader.CustomHats.Keys],
        [.. _visorLoader.CustomVisors.Keys],
        [.. _nameplateLoader.CustomNamePlates.Keys]
    );
    
    public void Reset()
    {
        _hatLoader.CustomHats.Clear();
        _visorLoader.CustomVisors.Clear();
        _nameplateLoader.CustomNamePlates.Clear();
        CosmeticGroup.ids.Clear();
        _loaded = false;
        Main.Logger.LogInfo("[Cosmetics] CosmeticsManager reset.");
    }
}
