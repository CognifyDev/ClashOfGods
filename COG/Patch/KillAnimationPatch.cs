namespace COG.Patch;

[HarmonyPatch]
internal static class KillAnimationPatch
{
    public static bool DisableNextAnim;

    [HarmonyPatch(typeof(KillAnimation._CoPerformKill_d__2), nameof(KillAnimation._CoPerformKill_d__2.MoveNext))]
    [HarmonyPrefix]
    private static bool AnimationPerformPatch(ref bool __result)
    {
        if (DisableNextAnim)
        {
            __result = false;
            DisableNextAnim = false;
            return false;
        }

        return true;
    }
}