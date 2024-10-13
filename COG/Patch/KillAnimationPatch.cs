using System;

namespace COG.Patch;

[HarmonyPatch]
internal static class KillAnimationPatch
{
    public static NetworkedPlayerInfo? NextKillerToBeReplaced { get; set; }

    [HarmonyPatch(typeof(KillOverlay), nameof(KillOverlay.ShowKillAnimation), new Type[]
    {
        typeof(NetworkedPlayerInfo), typeof(NetworkedPlayerInfo)
    })]
    [HarmonyPrefix]
    static void AnimationPerformPatch(ref NetworkedPlayerInfo killer)
    {
        if (NextKillerToBeReplaced)
        {
            killer = NextKillerToBeReplaced!;
            NextKillerToBeReplaced = null;
        }
    }
}