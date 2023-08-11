using System.Linq;
using COG.Listener;
using COG.UI.CustomButtons;
using InnerNet;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
internal class HostChatPatch
{
    public static bool Prefix(ChatController __instance)
    {
        if (__instance.freeChatField.textArea.text == "") return false;

        var returnAble = false;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            if (!listener.OnHostChat(__instance) && !returnAble)
                returnAble = true;

        if (returnAble) return false;

        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl))]
internal class PlayerKillPatch
{
    [HarmonyPatch(nameof(PlayerControl.CheckMurder))]
    [HarmonyPrefix]
    public static bool CheckMurderPath(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return false;

        var returnAble = false;
        foreach (var unused in ListenerManager.GetManager().GetListeners()
                     .Where(listener => !listener.OnPlayerMurder(__instance, target) && !returnAble)) returnAble = true;

        return !returnAble;
    }

    [HarmonyPatch(nameof(PlayerControl.MurderPlayer))]
    [HarmonyPostfix]
    public static void MurderPath(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.OnMurderPlayer(__instance, target);
    }
}

[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
internal class ChatUpdatePatch
{
    public static void Postfix(ChatController __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.OnChatUpdate(__instance);
    }
}

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
internal class HostSartPatch
{
    public static float timer = 600;
    private static string currentText = "";
    private static bool update;

    public static void Prefix(GameStartManager __instance)
    {
        // start with no limit
        GameStartManager.Instance.MinPlayers = 1;
        {
            // showtime
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance ||
                AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return;
            update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
        }
    }

    public static void Postfix(GameStartManager __instance)
    {
        // showtime
        if (update) currentText = __instance.PlayerCounter.text;
        if (!AmongUsClient.Instance.AmHost) return;
        timer = Mathf.Max(0f, timer -= Time.deltaTime);
        var minutes = (int)timer / 60;
        var seconds = (int)timer % 60;

        var suffix = $"({minutes:00}:{seconds:00})";
        __instance.PlayerCounter.text = currentText + suffix;
        __instance.PlayerCounter.autoSizeTextContainer = true;
    }
}

[HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
internal class ExileControllerPatch
{
    public static void Postfix(ExileController __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.OnPlayerExile(__instance);
        foreach (var btn in CustomButtonManager.GetManager().GetButtons())
            if (btn != null)
                btn.OnMeetingEndSpawn();
    }
}

[HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
internal class AirshipExileControllerPatch
{
    public static void Postfix(AirshipExileController __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.OnAirshipPlayerExile(__instance);
        foreach (var btn in CustomButtonManager.GetManager().GetButtons())
            if (btn != null)
                btn.OnMeetingEndSpawn();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
internal class OnGameJoinedPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] string gameIdString)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.OnGameJoined(__instance, gameIdString);
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
internal class OnPlayerLeftPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data,
        [HarmonyArgument(1)] DisconnectReasons reason)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.OnPlayerLeft(__instance, data, reason);
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
internal class OnPlayerJoinedPatch
{
    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.OnPlayerJoin(__instance, data);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
internal class ReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
    {
        var returnAble = false;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            if (!listener.OnPlayerReportDeadBody(__instance, target))
                returnAble = true;

        if (returnAble) return false;

        return true;
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
internal class FixedUpdatePatch
{
    public static void Postfix(PlayerControl __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            listener.AfterPlayerFixedUpdate(__instance);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
internal class CheckMurderPatch
{
    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        var returnAble = true;
        foreach (var unused in ListenerManager.GetManager().GetListeners()
                     .Where(listener => !listener.OnCheckMurder(__instance, target))) returnAble = false;
        return returnAble;
    }
}