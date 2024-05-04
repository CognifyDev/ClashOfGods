using COG.States;

namespace COG.Patch;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
class ServerUpdatePatch
{
    static void Postfix(ref int __result)
    {
        if (GameStates.IsOnlineGame)
        {
            // Changing server version for AU mods
            __result += 25;
        }
    }
}

[HarmonyPatch(typeof(Constants), nameof(Constants.IsVersionModded))]
public static class IsVersionModdedPatch
{
    public static bool Prefix(ref bool __result)
    {
        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
public static class DestroyQuickButton
{
    public static void Prefix(ChatController __instance)
    {
        if (__instance.quickChatButton.gameObject.active)
            __instance.quickChatButton.gameObject.SetActive(false);
    }
}