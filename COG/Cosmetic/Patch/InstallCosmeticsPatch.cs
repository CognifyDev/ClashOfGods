using HarmonyLib;

namespace COG.Cosmetics.Patch;

/// <summary>
///     Patches <c>ReferenceDataManager._Initialize_d__7.MoveNext</c> so that custom
///     cosmetics are injected into <c>ReferenceData</c> the moment the game finishes
///     loading its own catalogue — exactly when the HatManager will consume it.
/// </summary>
[HarmonyPatch(
    typeof(ReferenceDataManager._Initialize_d__7),
    nameof(ReferenceDataManager._Initialize_d__7.MoveNext))]
public static class InstallCosmeticsPatch
{
    // Guard so the injection runs exactly once per session even if MoveNext is called again.
    private static bool _installed;

    public static void Postfix(ReferenceDataManager._Initialize_d__7 __instance)
    {
        // __1__state < 0 means the coroutine has completed (state machine convention).
        if (__instance.__1__state >= 0 || _installed) return;

        _installed = true;

        Main.Logger.LogInfo("[Cosmetics] ReferenceDataManager initialised — installing cosmetics.");
        CosmeticsManager.Instance.InstallCosmetics(__instance.__4__this.Refdata);
    }
}
