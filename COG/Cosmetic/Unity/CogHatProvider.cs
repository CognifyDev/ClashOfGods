using System;
using COG.Cosmetics;
using Il2CppInterop.Runtime.Injection;
using Reactor.Utilities.Attributes;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace COG.Cosmetics.Unity;


[RegisterInIl2Cpp(typeof(IResourceProvider))]
public class CogHatProvider : ResourceProviderBase
{
    private static CogHatProvider?  _instance;
    private static IResourceProvider? _provider;
    
    public static void Initialize()
    {
        _instance = new CogHatProvider();
        _provider = new IResourceProvider(_instance.Pointer);
        Addressables.ResourceManager.ResourceProviders.Insert(0, _provider);
        Main.Logger.LogInfo("[Cosmetics] CogHatProvider registered with Addressables.");
    }
    
    public CogHatProvider(IntPtr ptr) : base(ptr) { }

    public CogHatProvider() : base(ClassInjector.DerivedConstructorPointer<CogHatProvider>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }

    public override bool CanProvide(Il2CppSystem.Type t, IResourceLocation location)
        => location.InternalId.StartsWith("cog.cosmetic.");

    public override Il2CppSystem.Type GetDefaultType(IResourceLocation location)
        => location.ResourceType;

    public override void Provide(ProvideHandle handle)
    {
        var internalId = handle.Location.InternalId;

        if (!internalId.StartsWith("cog.cosmetic."))
        {
            handle.Complete<UnityEngine.Object>(null!, false,
                new Il2CppSystem.Exception("Not a COG cosmetic"));
            return;
        }

        var parts = internalId.Split('/');
        if (parts.Length != 2)
        {
            Main.Logger.LogError($"[Cosmetics] Invalid id format: {internalId}");
            handle.Complete<UnityEngine.Object>(null!, false,
                new Il2CppSystem.Exception("Invalid COG cosmetic id"));
            return;
        }

        var id   = parts[0];
        var type = parts[1];

        if (!CosmeticsManager.Instance.ProvideCosmetic(handle, id, type, out var ex))
        {
            Main.Logger.LogError($"[Cosmetics] Failed to provide {id}/{type}: {ex}");
            handle.Complete<UnityEngine.Object>(null!, false,
                new Il2CppSystem.Exception(ex?.ToString() ?? "Unknown error"));
        }
    }

    public override void Release(IResourceLocation location, Il2CppSystem.Object obj)
    {
        // Cosmetic objects are kept alive by DontUnloadUnusedAsset; no action needed.
    }
}
