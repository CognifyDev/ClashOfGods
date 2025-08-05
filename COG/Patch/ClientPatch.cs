using AmongUs.GameOptions;
using COG.States;
using UnityEngine;

// ReSharper disable InconsistentNaming
// ReSharper disable RedundantAssignment

namespace COG.Patch;

[HarmonyPatch(typeof(Constants), nameof(Constants.GetBroadcastVersion))]
internal class ServerUpdatePatch
{
    private static void Postfix(ref int __result)
    {
        if (GameStates.IsOnlineGame)
            // Changing server version for AU mods
            __result += 25;
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

[HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Update))]
public static class HideUselessButtons
{
    public static void Postfix()
    {
        var gameObject = GameObject.Find("OptionsMenu/DataTab/TwitchLinkButton");
        gameObject?.SetActive(false);
    }
}

[HarmonyPatch(typeof(MMOnlineManager), nameof(MMOnlineManager.Start))]
static class LocalGamePatch
{
    static void Postfix()
    {
        GameObject.Find("CreateHnSGameButton").SetActive(false);
    }
}