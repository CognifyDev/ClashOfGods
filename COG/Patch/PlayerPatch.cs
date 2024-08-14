using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.AuClient;
using COG.Listener.Event.Impl.Player;
using COG.Listener.Event.Impl.PPhysics;
using COG.UI.CustomButton;
using COG.Utils;
using InnerNet;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
internal class LocalPlayerChatPatch
{
    public static bool Prefix(PlayerControl __instance, string chatText)
    {
        if (chatText is "" or null || !__instance.IsSamePlayer(PlayerControl.LocalPlayer)) return false;
        return ListenerManager.GetManager().ExecuteHandlers(
            new LocalPlayerChatEvent(__instance, chatText),
            EventHandlerType.Prefix);
    }

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] string chatText)
    {
        if (chatText is null or "" || __instance.IsSamePlayer(PlayerControl.LocalPlayer)) return;
        ListenerManager.GetManager().ExecuteHandlers(
            new LocalPlayerChatEvent(__instance, chatText),
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
        return ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerMurderEvent(__instance, target), EventHandlerType.Prefix);
    }

    [HarmonyPatch(nameof(PlayerControl.MurderPlayer))]
    [HarmonyPostfix]
    public static void MurderPath(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
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
        var result = ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerShapeShiftEvent(__instance, target, shouldAnimate), EventHandlerType.Prefix);
        if (!result)
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

[HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
internal class HostStartPatch
{
    public static float Timer = 600;
    private static string _currentText = "";
    private static bool _update;

    public static void Prefix(GameStartManager __instance)
    {
        if (GlobalCustomOptionConstant.DebugMode.GetBool())
        {
            // start with no limit
            __instance.MinPlayers = 1;
            __instance.StartButton.SetButtonEnableState(true);
            __instance.StartButton.ChangeButtonText(TranslationController.Instance.GetString(StringNames.StartLabel));
        }
            

        {
            // showtime
            if (!AmongUsClient.Instance.AmHost || !GameData.Instance ||
                AmongUsClient.Instance.AmLocalHost) return;
            _update = GameData.Instance.PlayerCount != __instance.LastPlayerCount;
        }
    }

    public static void Postfix(GameStartManager __instance)
    {
        // showtime
        //if (_update) _currentText = __instance.PlayerCounter.text;
        //if (!AmongUsClient.Instance.AmHost) return;
        //Timer = Mathf.Max(0f, Timer -= Time.deltaTime);
        //var minutes = (int)Timer / 60;
        //var seconds = (int)Timer % 60;

        //var suffix = $"({minutes:00}:{seconds:00})";
        //__instance.PlayerCounter.text = _currentText + suffix;
        //__instance.PlayerCounter.autoSizeTextContainer = true;
    }
}

[HarmonyPatch(typeof(ExileController), nameof(ExileController.WrapUp))]
internal class ExileControllerWrapUpPatch
{
    public static bool Prefix(ExileController __instance)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerExileEndEvent(__instance.initData.networkedPlayer.Object, __instance),
                EventHandlerType.Prefix);
    }

    public static void Postfix(ExileController __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerExileEndEvent(__instance.initData.networkedPlayer.Object, __instance),
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
                new PlayerExileEndOnAirshipEvent(__instance.initData.networkedPlayer, __instance),
                EventHandlerType.Prefix);
    }

    public static void Postfix(AirshipExileController __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(
                new PlayerExileEndOnAirshipEvent(__instance.initData.networkedPlayer, __instance),
                EventHandlerType.Postfix);
        foreach (var btn in CustomButtonManager.GetManager().GetButtons()) btn.OnMeetingEndSpawn();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
internal class OnGameJoinedPatch
{
    public static bool Prefix(AmongUsClient __instance, [HarmonyArgument(0)] string gameIdString)
    {
        return ListenerManager.GetManager().ExecuteHandlers(new AmongUsClientJoinLobbyEvent(__instance, gameIdString),
            EventHandlerType.Prefix);
    }

    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] string gameIdString)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new AmongUsClientJoinLobbyEvent(__instance, gameIdString), EventHandlerType.Postfix);
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
            .ExecuteHandlers(new AmongUsClientPlayerJoinEvent(__instance, data), EventHandlerType.Prefix);
    }

    public static void Postfix(AmongUsClient __instance, [HarmonyArgument(0)] ClientData data)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new AmongUsClientPlayerJoinEvent(__instance, data), EventHandlerType.Postfix);
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
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo? target)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerReportDeadBodyEvent(__instance, target), EventHandlerType.Prefix);
    }

    public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] NetworkedPlayerInfo? target)
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

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.AdjustLighting))]
internal class AdjustLightPatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerAdjustLightingEvent(__instance), EventHandlerType.Prefix);
    }

    public static void Postfix(PlayerControl __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerAdjustLightingEvent(__instance), EventHandlerType.Postfix);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Awake))]
internal class PlayerControlAwakePatch
{
    public static bool Prefix(PlayerControl __instance)
    {
        return ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerControlAwakeEvent(__instance), EventHandlerType.Prefix);
    }

    public static void Postfix(PlayerControl __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new PlayerControlAwakeEvent(__instance), EventHandlerType.Postfix);
    }
}