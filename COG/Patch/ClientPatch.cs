using AmongUs.GameOptions;
using COG.States;
using Reactor.Utilities.Extensions;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment

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
    public static void Postfix(ChatController __instance)
    {
        var gameObject = __instance.quickChatButton.gameObject;
        if (gameObject == null || gameObject.IsDestroyedOrNull())
            gameObject!.DestroyImmediate();
    }
}

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Update))]
public static class DestroyUselessButtons
{
    public static void Postfix()
    {
        var gameObject = GameObject.Find("OptionsMenu/DataTab/TwitchLinkButton");
        if (gameObject != null && !gameObject.IsDestroyedOrNull())
        {
            gameObject.DestroyImmediate();
        }
    }
}

[HarmonyPatch(typeof(GameOptionsManager), nameof(GameOptionsManager.SwitchGameMode))]
public static class NormalModeOnly
{
    public static void Postfix(GameOptionsManager __instance)
    {
        if (__instance.currentGameMode != GameModes.Normal)
        {
            __instance.SwitchGameMode(GameModes.Normal);
        }
    }
}