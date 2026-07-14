using HarmonyLib;

namespace COG.Cosmetics.Patch;

[HarmonyPatch(typeof(ReferenceDataManager), "SetStoreVersions")]
public static class InstallCosmeticsPatch
{
    private static bool _installed;

    public static void Postfix(ReferenceDataManager __instance)
    {
        if (_installed) return;

        _installed = true;

        Main.Logger.LogInfo("ReferenceDataManager initialised — installing cosmetics.");
        CosmeticsManager.Instance.LoadCosmetics();
        CosmeticsManager.Instance.InstallCosmetics(__instance.Refdata);
    }
}
