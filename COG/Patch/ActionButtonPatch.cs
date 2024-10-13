namespace COG.Patch;

[HarmonyPatch]
internal static class ActionButtonPatch
{
    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.Update))]
    [HarmonyPrefix]
    static bool AbilityButtonUpdatePatch() => false; // Prevent showing comms down sprite on the buttons
}