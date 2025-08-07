namespace COG.Patch;

[HarmonyPatch]
internal static class KillAnimationPatch
{
    public static NetworkedPlayerInfo? NextKillerToBeReplaced { get; set; }

    [HarmonyPatch(typeof(KillOverlay), nameof(KillOverlay.ShowKillAnimation), typeof(NetworkedPlayerInfo),
        typeof(NetworkedPlayerInfo))]
    [HarmonyPrefix]
    private static void AnimationPerformPatch(ref NetworkedPlayerInfo killer)
    {
        if (NextKillerToBeReplaced)
        {
            killer = NextKillerToBeReplaced!;
            NextKillerToBeReplaced = null;
        }
    }
}