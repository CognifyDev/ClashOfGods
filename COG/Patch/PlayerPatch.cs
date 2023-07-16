using COG.Listener;
using HarmonyLib;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
class HostChatPatch
{
    public static bool Prefix(ChatController __instance)
    {
        if (__instance.TextArea.text == "") return false;

        bool returnAble = false;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            if (!listener.OnHostChat(__instance) && !returnAble)
            {
                returnAble = true;
            }
        }

        if (returnAble) return false;

        return true;
    }
}


[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
class PlayerMurderPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        
        bool returnAble = false;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            if (!listener.OnPlayerMurder(__instance, target) && !returnAble)
            {
                returnAble = true;
            }
        }

        if (returnAble) return false;

        return true;
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
internal class ChatUpdatePatch
{
    public static void Postfix(ChatController __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            listener.OnChatUpdate(__instance);
        }
    }
}
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
class HostSartPatch
{
    public static float timer = 600;
    private static string currentText = "";
    private static bool update = false;
    public static void Prefix(GameStartManager __instance)
    {
        // showtime
        if (!AmongUsClient.Instance.AmHost || !GameData.Instance || AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return;
        update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
        // start with no limit
        GameStartManager.Instance.MinPlayers = 1;
    }
    public static void Postfix(GameStartManager __instance)
    {
        // showtime
        if (update) currentText = __instance.PlayerCounter.text;
        if (!AmongUsClient.Instance.AmHost) return;
        timer = Mathf.Max(0f, timer -= Time.deltaTime);
        int minutes = (int)timer / 60;
        int seconds = (int)timer % 60;

        string suffix = $"({minutes:00}:{seconds:00})";
        __instance.PlayerCounter.text = currentText + suffix;
        __instance.PlayerCounter.autoSizeTextContainer = true;
    }
}

[HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
class ExileControllerPatch
{
    public static void Postfix(ExileController __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            listener.OnPlayerExile(__instance);
        }
    }
}

[HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
class AirshipExileControllerPatch
{
    public static void Postfix(AirshipExileController __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            listener.OnAirshipPlayerExile(__instance);
        }
    }
}