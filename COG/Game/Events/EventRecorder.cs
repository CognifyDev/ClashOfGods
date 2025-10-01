using COG.Listener.Event.Impl.Game;
using COG.Listener.Event.Impl.Game.Record;
using COG.Rpc;
using COG.Utils;
using COG.Utils.Coding;
using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace COG.Game.Events;

[WorkInProgress]
public class EventRecorder
{
    private readonly List<IGameEvent> _events;

    public EventRecorder()
    {
        Instance = this;
        _events = new List<IGameEvent>();
    }

    public static EventRecorder Instance { get; private set; } = null!;

    public IEnumerable<IGameEvent?> GetEvents()
    {
        return _events;
    }

    public void Record(IGameEvent? gameEvent)
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

    [FixMe("BUG")]
    public void RpcRecord<T>(NetworkedGameEventBase<T> gameEvent) where T : INetworkedGameEventSender, new()
    {
        var writer = RpcWriter.Start(KnownRpc.SyncGameEvent);
        dynamic sender = gameEvent.EventSender;
        writer.Write(sender.Id);
        //sender.Serialize(writer, gameEvent);
        writer.Finish();
    }

    public static void ResetAll()
    {
        Instance = null!;
        Main.Logger.LogInfo("Event data have been reset.");
    }
}

// 原版游戏死亡方式：击杀，断联，驱逐
// 以上三者都会调用Die方法
// 修补Die方法会将问题复杂化
[HarmonyPatch]
public static class GameEventPatch // not use listener for flexibility in patching
{
    #region 原版三种死亡方式
    [HarmonyPatch(typeof(GameData), nameof(GameData.HandleDisconnect),
        typeof(PlayerControl), typeof(DisconnectReasons))]
    [HarmonyPrefix] // player might be null if we use postfix
    private static void HandleDisconnectPatch(PlayerControl player)
    {
        EventRecorder.Instance.Record(new PlayerDisconnectGameEvent(player.GetPlayerData()!));
    }

    internal readonly static List<CustomPlayerData> DisableOnceDeath = new();

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
    [HarmonyPostfix]
    private static void MurderPlayerPostfixPatch(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target, [HarmonyArgument(1)] MurderResultFlags resultFlags)
    {
        if (!resultFlags.HasFlag(MurderResultFlags.Succeeded)) return;
        var targetCached = target.GetPlayerData();

        if (DisableOnceDeath.Contains(targetCached))
        {
            DisableOnceDeath.Remove(targetCached);
            return;
        }

        EventRecorder.Instance.Record(new PlayerKillGameEvent(__instance.GetPlayerData(), target.GetPlayerData()));
    }

    [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
    [HarmonyPostfix]
    private static void ExilePatch([HarmonyArgument(0)] ExileController.InitProperties init)
    {
        if (!init.networkedPlayer) return;
        EventRecorder.Instance.Record(new PlayerExileGameEvent(init.networkedPlayer.GetPlayerData()));
    }
    #endregion

    [HarmonyPatch(typeof(PlayerPhysics._CoEnterVent_d__47), nameof(PlayerPhysics._CoEnterVent_d__47.MoveNext))]
    [HarmonyPostfix] // not patch CoEnterVent as it is inlined
    private static void EnterVentPatch(PlayerPhysics._CoEnterVent_d__47 __instance, bool __result)
    {
        if (!__result) return; // only record if player actually entered the vent
        var vent = __instance._vent_5__2;
        var playerPhysics = __instance.__4__this;
        EventRecorder.Instance.Record(new EnterVentGameEvent(playerPhysics.myPlayer.GetPlayerData()!, vent.Id));
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CompleteTask))]
    [HarmonyPostfix]
    private static void CompleteTaskPatch(PlayerControl __instance, [HarmonyArgument(1)] uint idx)
    {
        EventRecorder.Instance.Record(new FinishTaskGameEvent(__instance.GetPlayerData()!, idx));
    }


    [HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
    [HarmonyPostfix]
    private static void StartGamePatch()
    {
        EventRecorder.Instance.Record(new StartGameEvent());
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Revive))]
    [HarmonyPostfix]
    private static void RevivePatch(PlayerControl __instance)
    {
        EventRecorder.Instance.Record(new PlayerReviveGameEvent(__instance.GetPlayerData()!));
    }

    [HarmonyPatch(typeof(ShipStatus._CoStartMeeting_d__102), nameof(ShipStatus._CoStartMeeting_d__102.MoveNext))]
    [HarmonyPostfix] // CoStartMeeting also inlined
    private static void StartMeetingPatch(ShipStatus._CoStartMeeting_d__102 __instance, bool __result)
    {
        if (!__result) return;
        EventRecorder.Instance.Record(new StartMeetingEvent(__instance.reporter.GetPlayerData(),
            __instance.target.GetPlayerData()));
    }

}