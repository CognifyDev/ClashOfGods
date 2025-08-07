namespace COG.Patch;

[HarmonyPatch]
internal static class ActionButtonPatch
{
    [HarmonyPatch(typeof(AbilityButton), nameof(AbilityButton.Update))]
    [HarmonyPrefix]
    private static bool AbilityButtonUpdatePatch()
    {
        return false;
        // Prevent showing comms down sprite on the buttons
    }
}