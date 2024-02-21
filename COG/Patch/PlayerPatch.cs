using COG.Listener;
using COG.Listener.Event.Impl.AuClient;
using COG.Listener.Event.Impl.Player;
using COG.Listener.Event.Impl.PPhysics;
using COG.States;
using COG.UI.CustomButton;
using COG.Utils;
using InnerNet;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
internal class LocalPlayerChatPatch
{
    public static bool Prefix(ChatController __instance)
    {
        return __instance.freeChatField.textArea.text is not (null or "")
               && ListenerManager.GetManager().ExecuteHandlers(
                   new LocalPlayerChatEvent(PlayerControl.LocalPlayer, __instance),
                   EventHandlerType.Prefix);
    }

    public static void Postfix(ChatController __instance)
    {
        if (__instance.freeChatField.textArea.text is null or "") return;
        ListenerManager.GetManager().ExecuteHandlers(
            new LocalPlayerChatEvent(PlayerControl.LocalPlayer, __instance),
            EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(PlayerControl))]
internal class PlayerKillPatch
{
    [HarmonyPatch(nameof(PlayerControl.CheckMurder))]
    [HarmonyPrefix]
    public static bool CheckMurderPath(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return true;
        return ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerMurderEvent(__instance, target), EventHandlerType.Prefix);
    }

    [HarmonyPatch(nameof(PlayerControl.MurderPlayer))]
    [HarmonyPostfix]
    public static void MurderPath(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerMurderEvent(__instance, target), EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(PlayerControl))]
internal class PlayerShapeShiftPatch
{
    [HarmonyPatch(nameof(PlayerControl.CheckShapeshift))]
    [HarmonyPrefix]
    public static bool CheckShapeShiftPatch(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target,
        [HarmonyArgument(1)] bool shouldAnimate)
    {
        if (!AmongUsClient.Instance.AmHost) return true;

        var result = ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerShapeShiftEvent(__instance, target, shouldAnimate), EventHandlerType.Prefix);
        if (result)
            __instance.RpcRejectShapeshift();
        return result;
    }

    [HarmonyPatch(nameof(PlayerControl.Shapeshift))]
    [HarmonyPostfix]
    public static void ShapeShiftPatch(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target,
        [HarmonyArgument(1)] bool shouldAnimate)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerShapeShiftEvent(__instance, target, shouldAnimate), EventHandlerType.Postfix);
    }
}

/*
[HarmonyPatch(typeof(ChatController), nameof(ChatController.Update))]
internal class ChatUpdatePatch
{
    public static void Postfix(ChatController __instance)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.OnChatUpdate(__instance);
    }
}
*/
[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
internal class HostStartPatch
{
    public static float Timer = 600;
    private static string _currentText = "";
    private static bool _update;

    public static void Prefix(GameStartManager __instance)
    {
        if (GlobalCustomOption.DebugMode.GetBool())
            // start with no limit
            GameStartManager.Instance.MinPlayers = 1;

        {
            // showtime
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance ||
                !__instance.GameRoomNameCode.isActiveAndEnabled) return;
            _update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
        }
    }

    public static void Postfix(GameStartManager __instance)
    {
        // showtime
        if (_update) _currentText = __instance.PlayerCounter.text;
        if (!AmongUsClient.Instance.AmHost) return;
        Timer = Mathf.Max(0f, Timer -= Time.deltaTime);
        var minutes = (int)Timer / 60;
        var seconds = (int)Timer % 60;

        var suffix = $"({minutes:00}:{seconds:00})";
        __instance.PlayerCounter.text = _currentText + suffix;
        __instance.PlayerCounter.autoSizeTextContainer = true;
    }
}

[HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
internal class ExileControllerPatch
{
    public static bool Prefix(ExileController __instance)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerExileEvent(PlayerUtils.GetPlayerById(__instance.exiled.PlayerId)!, __instance),
                EventHandlerType.Prefix);
    }

    public static void Postfix(ExileController __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerExileEvent(PlayerUtils.GetPlayerById(__instance.exiled.PlayerId)!, __instance),
                EventHandlerType.Postfix);
        foreach (var btn in CustomButtonManager.GetManager().GetButtons()) btn.OnMeetingEndSpawn();
    }
}

[HarmonyPatch(typeof(AirshipExileController), nameof(AirshipExileController.WrapUpAndSpawn))]
internal class AirshipExileControllerPatch
{
    public static bool Prefix(AirshipExileController __instance)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(
                new PlayerExileOnAirshipEvent(PlayerUtils.GetPlayerById(__instance.exiled.PlayerId)!, __instance),
                EventHandlerType.Prefix);
    }

    public static void Postfix(AirshipExileController __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(
                new PlayerExileOnAirshipEvent(PlayerUtils.GetPlayerById(__instance.exiled.PlayerId)!, __instance),
                EventHandlerType.Postfix);
        foreach (var btn in CustomButtonManager.GetManager().GetButtons()) btn.OnMeetingEndSpawn();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
internal class OnGameJoinedPatch
{
    public static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] string gameIdString)
    {
        return ListenerManager.GetManager().ExecuteHandlers(new LocalAmongUsClientJoinEvent(__instance, gameIdString),
            EventHandlerType.Prefix);
    }

    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] string gameIdString)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new LocalAmongUsClientJoinEvent(__instance, gameIdString), EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
internal class OnPlayerLeftPatch
{
    public static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data,
        [HarmonyArgument(1)] DisconnectReasons reason)
    {
        return ListenerManager.GetManager().ExecuteHandlers(new AmongUsClientLeaveEvent(__instance, data, reason),
            EventHandlerType.Prefix);
    }

    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data,
        [HarmonyArgument(1)] DisconnectReasons reason)
    {
        ListenerManager.GetManager().ExecuteHandlers(new AmongUsClientLeaveEvent(__instance, data, reason),
            EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
internal class OnPlayerJoinedPatch
{
    public static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(new AmongUsClientJoinEvent(__instance, data), EventHandlerType.Prefix);
    }

    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new AmongUsClientJoinEvent(__instance, data), EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CreatePlayer))]
internal class CreatePlayerPatch
{
    public static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(new AmongUsClientCreatePlayerEvent(__instance, client), EventHandlerType.Prefix);
    }

    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData client)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new AmongUsClientCreatePlayerEvent(__instance, client), EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.CoSpawnPlayer))]
internal class OnSpawnPlayerPatch
{
    public static bool Prefix(PlayerPhysics __instance, [HarmonyArgument(0)] LobbyBehaviour lobbyBehaviour)
    {
        return ListenerManager.GetManager().ExecuteHandlers(new PlayerPhysicsCoSpawnEvent(__instance, lobbyBehaviour),
            EventHandlerType.Prefix);
    }

    public static void Postfix(PlayerPhysics __instance, [HarmonyArgument(0)] LobbyBehaviour lobbyBehaviour)
    {
        ListenerManager.GetManager().ExecuteHandlers(new PlayerPhysicsCoSpawnEvent(__instance, lobbyBehaviour),
            EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
internal class ReportDeadBodyPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo? target)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerReportDeadBodyEvent(__instance, target), EventHandlerType.Prefix);
    }

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo? target)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerReportDeadBodyEvent(__instance, target), EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
internal class FixedUpdatePatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerFixedUpdateEvent(__instance), EventHandlerType.Prefix);
    }

    public static void Postfix(PlayerControl __instance)
    {
        ListenerManager.GetManager().ExecuteHandlers(new PlayerFixedUpdateEvent(__instance), EventHandlerType.Postfix);
    }
}