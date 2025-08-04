using COG.Game.Events.Impl;
using COG.Game.Events.Impl.Handlers;
using COG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace COG.Game.Events;

public class EventRecorder
{
    public static EventRecorder Instance { get; private set; } = null!;
    
    private readonly List<IGameEvent> _events;
    private readonly List<IEventHandler> _handlers;

    public EventRecorder()
    {
        Instance = this;
        _events = new();
        _handlers = new();

        _handlers.AddRange(new IEventHandler[]
        {
            new PlayerDeathHandler(),
            new PlayerKillHandler()
        });
    }

    public IEnumerable<IGameEvent> GetEvents() => _events;

    public void Record(IGameEvent gameEvent)
    {
        if (gameEvent != null)
        {
            _events.Add(gameEvent);
            Main.Logger.LogInfo($"Recorded game event: {gameEvent.GetType().Name}");
        }
        else
        {
            Main.Logger.LogWarning("Attempted to record an illegal game event.");
        }
    }

    public IGameEvent RecordTypeEvent(GameEventType type, CustomPlayerData player, params object[] extraArguments)
    {
        var gameEvent = HandleTypeEvent(type, player, extraArguments);
        Record(gameEvent);
        return gameEvent;
    }

    public IGameEvent HandleTypeEvent(GameEventType type, CustomPlayerData player, params object[] extraArguments) => _handlers.FirstOrDefault(h => h.EventType == type)?.Handle(player, extraArguments) ?? null!;

    public static void ResetAll()
    {
        Instance = null!;
        Main.Logger.LogInfo("Event data has been reset.");
    }
}

[HarmonyPatch]
public static class GameEventPatch // not use listener for flexibility in patching
{
    [HarmonyPatch(typeof(PlayerPhysics._CoEnterVent_d__47), nameof(PlayerPhysics._CoEnterVent_d__47.MoveNext))]
    [HarmonyPostfix] // not patch CoEnterVent as it is inlined
    static void EnterVentPatch(PlayerPhysics._CoEnterVent_d__47 __instance, bool __result)
    {
        if (!__result) return; // only record if player actually entered the vent
        var vent = __instance._vent_5__2;
        var playerPhysics = __instance.__4__this;
        EventRecorder.Instance.Record(new EnterVentGameEvent(playerPhysics.myPlayer.GetPlayerData()!, vent.Id));
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
    [HarmonyPostfix]
    static void CompleteTaskPatch(PlayerControl __instance, [HarmonyArgument(1)] uint idx)
    {
        EventRecorder.Instance.Record(new FinishTaskGameEvent(__instance.GetPlayerData()!, idx));
    }

    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect), typeof(PlayerControl), typeof(DisconnectReasons))]
    [HarmonyPrefix] // player might be null if we use postfix
    static void HandleDisconnectPatch(PlayerControl player)
    {
        EventRecorder.Instance.Record(new PlayerDisconnectGameEvent(player.GetPlayerData()!));
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    [HarmonyPrefix]
    static void MurderPlayerPrefixPatch(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, ref IGameEvent __state)
    {
        EventRecorder.Instance.Record(__state = EventRecorder.Instance.HandleTypeEvent(GameEventType.Kill, __instance.GetPlayerData(), target.GetPlayerData(), ExtraMessage!));
    }

    private static IGameEvent? _dieEvent = null;

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    [HarmonyPostfix]
    static void MurderPlayerPostfixPatch(IGameEvent __state) // PlayerControl.Die is called before MurderPlayer is done
    {
        ((PlayerKillGameEvent)__state).RelatedDeathEvent = _dieEvent!;
    }

    public static string? ExtraMessage { get; set; } = null;

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
    [HarmonyPostfix]
    static void DiePatch(PlayerControl __instance)
    {
        if (ExtraMessage != null)
            _dieEvent = EventRecorder.Instance.RecordTypeEvent(GameEventType.Die, __instance.GetPlayerData(), ExtraMessage);
        else
            _dieEvent = EventRecorder.Instance.RecordTypeEvent(GameEventType.Die, __instance.GetPlayerData());

        ExtraMessage = null;
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
    [HarmonyPostfix]
    static void StartGamePatch()
    {
        EventRecorder.Instance.Record(new GameStartsEvent());
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
    [HarmonyPostfix]
    static void RevivePatch(PlayerControl __instance)
    {
        EventRecorder.Instance.Record(new PlayerReviveGameEvent(__instance.GetPlayerData()!));
    }

    [HarmonyPatch(typeof(ShipStatus._CoStartMeeting_d__102), nameof(ShipStatus._CoStartMeeting_d__102.MoveNext))]
    [HarmonyPostfix] // CoStartMeeting also inlined
    static void StartMeetingPatch(ShipStatus._CoStartMeeting_d__102 __instance, bool __result)
    {
        if (!__result) return;
        EventRecorder.Instance.Record(new StartMeetingEvent(__instance.reporter.GetPlayerData(), __instance.target.GetPlayerData()));
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    [HarmonyPostfix]
    static void ExilePatch([HarmonyArgument(0)] ExileController.InitProperties init)
    {
        EventRecorder.Instance.Record(new PlayerExileGameEvent(init.networkedPlayer.GetPlayerData()));
    }
}
