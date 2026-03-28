using HarmonyLib;
using UnityEngine.AddressableAssets;

namespace COG.Cosmetics.Patch;


[HarmonyPatch(typeof(AssetReference), nameof(AssetReference.RuntimeKeyIsValid))]
public static class AssetReferenceKeyPatch
{
    public static bool Prefix(AssetReference __instance, ref bool __result)
    {
        if (!__instance.AssetGUID.StartsWith("cog.cosmetic.")) return true;
        __result = true;
        return false;
    }
}
