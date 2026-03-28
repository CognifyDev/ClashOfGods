using System;
using COG.Cosmetics;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace COG.Cosmetics.Unity;


[RegisterInIl2Cpp(typeof(IResourceLocator))]
public class CogHatLocator : Il2CppSystem.Object
{
    private static CogHatLocator? _instance;
    private static IResourceLocator? _locator;

    public static void Initialize()
    {
        _instance = new CogHatLocator();
        _locator  = new IResourceLocator(_instance.Pointer);
        Addressables.AddResourceLocator(_locator);
        Main.Logger.LogInfo("[Cosmetics] CogHatLocator registered with Addressables.");
    }

    public CogHatLocator(IntPtr ptr) : base(ptr) { }

    public CogHatLocator() : base(ClassInjector.DerivedConstructorPointer<CogHatLocator>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public string LocatorId => GetType().FullName!;

    public Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object>
        Keys => CosmeticsManager.Instance.EmptyKeys;

    private string ProviderId { get; } = typeof(CogHatProvider).FullName!;

    public static string GetGuid(string cosmeticId, string type) => $"{cosmeticId}/{type}";

    public bool Locate(
        Il2CppSystem.Object key,
        Il2CppSystem.Type   type,
        out Il2CppSystem.Collections.Generic.IList<IResourceLocation> locations)
    {
        locations = null!;

        var keyString = key.ToString();
        if (keyString is null || !keyString.StartsWith("cog.cosmetic."))
            return false;

        var parts = keyString.Split('/');
        if (parts.Length != 2)
        {
            Main.Logger.LogWarning($"[Cosmetics] Malformed cosmetic key: {keyString}");
            return false;
        }

        var realKey  = parts[0];
        var typeName = parts[1];

        if (!CosmeticsManager.Instance.LocateCosmetic(realKey, typeName, out var il2CppType))
        {
            Main.Logger.LogWarning($"[Cosmetics] Cosmetic not found: {realKey} / {typeName}");
            return false;
        }

        var location = new ResourceLocationBase(keyString, keyString, ProviderId, il2CppType);

        var list = new Il2CppSystem.Collections.Generic.List<ResourceLocationBase>();
        list.Add(location);
        locations = new Il2CppSystem.Collections.Generic.IList<IResourceLocation>(list.Pointer);
        return true;
    }
}
