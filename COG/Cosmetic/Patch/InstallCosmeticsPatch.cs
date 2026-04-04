using HarmonyLib;

namespace COG.Cosmetics.Patch;

[HarmonyPatch(
    typeof(ReferenceDataManager._Initialize_d__7),
    nameof(ReferenceDataManager._Initialize_d__7.MoveNext))]
public static class InstallCosmeticsPatch
{
    private static bool _installed;

    public static void Postfix(ReferenceDataManager._Initialize_d__7 __instance)
    {
        // __1__state < 0 means the coroutine has completed (state machine convention).
        if (__instance.__1__state >= 0 || _installed) return;

        _installed = true;
        
        Main.Logger.LogInfo("ReferenceDataManager initialised — installing cosmetics.");
        CosmeticsManager.Instance.LoadCosmetics();
        CosmeticsManager.Instance.InstallCosmetics(__instance.__4__this.Refdata);
    }
}
