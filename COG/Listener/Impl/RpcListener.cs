using System;
using System.Collections.Generic;
using System.Linq;
using COG.Game.Events;
using COG.Listener.Event.Impl.Player;
using COG.Role;
using COG.Rpc;
using COG.Rpc.Role;
using COG.UI.CustomOption;
using COG.UI.Hud.Meeting;
using COG.Utils;
using InnerNet;

namespace COG.Listener.Impl;

public class RpcListener : IListener
{
    private static readonly Dictionary<KnownRpc, Action<PlayerControl, MessageReader>> Handlers = new()
    {
        [KnownRpc.UpdateOption] = HandleUpdateOption,
        [KnownRpc.ShareOptions] = HandleShareOptions,
        [KnownRpc.SetRole] = HandleSetRole,
        [KnownRpc.NotifySettingChange] = HandleNotifySettingChange,
        [KnownRpc.KillWithoutDeadBody] = HandleKillWithoutDeadBody,
        [KnownRpc.Revive] = HandleRevive,
        [KnownRpc.HideDeadBody] = HandleHideDeadBody,
        [KnownRpc.GuessPlayer] = HandleGuessPlayer,
        [KnownRpc.Mark] = HandleMark,
        [KnownRpc.SyncRoleGameData] = HandleSyncRoleGameData,
        [KnownRpc.AdvancedMurder] = HandleAdvancedMurder,
        [KnownRpc.RoleRpc] = HandleRoleRpc,
        [KnownRpc.SyncGameEvent] = HandleSyncGameEvent,
    };

    [EventHandler(EventHandlerType.Postfix)]
    public void AfterRpcReceived(PlayerHandleRpcEvent @event)
    {
        var callId = @event.CallId;
        var reader = @event.Reader;

        if (Handlers.TryGetValue((KnownRpc)callId, out var handler))
            handler(@event.Player, reader);

        // Legacy IRpcHandler dispatch
        foreach (var h in IRpcHandler.Handlers)
        {
            if (h is RpcHandler rpc && rpc.CallId == callId)
                h.OnReceive(reader);
        }

        // Per-role RPC dispatch
        foreach (var cr in CustomRoleManager.GetManager().GetRoles())
            cr.OnRpcReceived(@event.Player, callId, reader);
    }

    // ──────────────────────────────────────────────────────────────
    //  Individual handlers
    // ──────────────────────────────────────────────────────────────

    private static void HandleUpdateOption(PlayerControl sender, MessageReader reader)
    {
        if (sender.GetClientID() != AmongUsClient.Instance.HostId)
        {
            Main.Logger.LogWarning(
                $"[CustomOption] Discarding UpdateOption from non-host client {sender.GetClientID()}.");
            return;
        }

        var id = reader.ReadPackedInt32();
        var selection = reader.ReadPackedInt32();
        CustomOption.Options.FirstOrDefault(o => o.Id == id)?.ApplySelectionSilently(selection);
    }

    private static void HandleShareOptions(PlayerControl sender, MessageReader reader)
    {
        var count = Math.Min(reader.ReadPackedUInt32(), 1000u);
        try
        {
            for (var i = 0; i < count; i++)
            {
                var optionId = reader.ReadPackedUInt32();
                var selection = reader.ReadPackedUInt32();
                CustomOption.Options.FirstOrDefault(o => o.Id == (int)optionId)
                    ?.ApplySelectionSilently((int)selection);
            }

            Main.Logger.LogInfo($"[CustomOption] Applied {count} option(s) from ShareOptions.");
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError("Error while deserializing options: " + e.Message);
        }
    }

    private static void HandleSetRole(PlayerControl sender, MessageReader reader)
    {
        var playerId = reader.ReadByte();
        var roleId = reader.ReadPackedInt32();
        var player = PlayerUtils.GetPlayerById(playerId);
        var role = CustomRoleManager.GetManager().GetRoleById(roleId);
        if (player != null && role != null)
            player.SetCustomRole(role, vanillaSync: false);
    }

    private static void HandleNotifySettingChange(PlayerControl sender, MessageReader reader)
    {
        var id = reader.ReadPackedInt32();
        var text = reader.ReadString();
        HudManager.Instance.Notifier.SettingsChangeMessageLogic((StringNames)id, text, true);
    }

    private static void HandleKillWithoutDeadBody(PlayerControl sender, MessageReader reader)
    {
        if (!GameStates.InRealGame) return;
        if (!sender.IsAlive()) return;

        var killer = reader.ReadNetObject<PlayerControl>();
        var target = reader.ReadNetObject<PlayerControl>();
        var showAnimationToEverybody = reader.ReadBoolean();
        var anonymousKiller = reader.ReadBoolean();

        if (killer != null && target != null && sender.PlayerId == killer.PlayerId)
            killer.KillWithoutDeadBody(target, showAnimationToEverybody, anonymousKiller);
    }

    private static void HandleRevive(PlayerControl sender, MessageReader reader)
    {
        if (!GameStates.InRealGame) return;
        var player = reader.ReadNetObject<PlayerControl>();
        if (player?.Data != null && player.Data.IsDead)
            player.Revive();
    }

    private static void HandleHideDeadBody(PlayerControl sender, MessageReader reader)
    {
        if (!GameStates.InRealGame) return;
        var pid = reader.ReadByte();
        var body = UnityEngine.Object.FindObjectsOfType<DeadBody>().ToList()
            .FirstOrDefault(b => b.ParentId == pid);
        if (!body) return;
        if (sender.IsAlive())
            body!.gameObject.SetActive(false);
    }

    private static void HandleGuessPlayer(PlayerControl sender, MessageReader reader)
    {
        if (!GameStates.InRealGame) return;
        var guesserId = reader.ReadByte();
        var targetId = reader.ReadByte();
        var roleId = reader.ReadPackedInt32();
        var killOnWrong = reader.ReadBoolean();
        var guesserPlayer = PlayerUtils.GetPlayerById(guesserId);
        var targetPlayer = PlayerUtils.GetPlayerById(targetId);
        if (guesserPlayer == null || targetPlayer == null) return;
        GuesserButton.GuessPlayer(guesserPlayer, targetPlayer, roleId, killOnWrong);
    }

    private static void HandleMark(PlayerControl sender, MessageReader reader)
    {
        var target = reader.ReadNetObject<PlayerControl>();
        var tag = reader.ReadString();
        var playerData = target?.GetPlayerData();
        if (playerData == null) return;

        if (tag.StartsWith(PlayerUtils.DeleteTagPrefix))
            playerData.Tags.Remove(tag.Replace(PlayerUtils.DeleteTagPrefix, ""));
        else
            playerData.Tags.Add(tag);
    }

    private static void HandleSyncRoleGameData(PlayerControl sender, MessageReader reader)
    {
        int roleId;
        CustomRole role;

        try
        {
            roleId = reader.ReadPackedInt32();
            role = CustomRoleManager.GetManager().GetRoleById(roleId)
                   ?? throw new System.Exception();
        }
        catch
        {
            Main.Logger.LogError("Got invalid roleId while synchronizing role data.");
            return;
        }

        Main.Logger.LogMessage($"Syncing game data for {role.Name}...");
        role.OnRoleGameDataGettingSynchronized(reader);
    }

    private static void HandleAdvancedMurder(PlayerControl sender, MessageReader reader)
    {
        sender.MurderAdvanced(AdvancedKillOptions.Deserialize(reader));
    }

    private static void HandleRoleRpc(PlayerControl sender, MessageReader reader)
    {
        RoleRpcManager.Dispatch(sender, reader);
    }

    private static void HandleSyncGameEvent(PlayerControl sender, MessageReader reader)
    {
        var eventNameFull = reader.ReadString();
        var typeNameFull = reader.ReadString();

        var eventType = Main.Assembly.GetTypes().FirstOrDefault(t => t.FullName == eventNameFull);
        var deserializerType = Main.Assembly.GetTypes().FirstOrDefault(t => t.FullName == typeNameFull);

        if (eventType == null || deserializerType == null)
        {
            Main.Logger.LogError($"Unsupported event type: {eventNameFull} ({typeNameFull})");
            return;
        }

        try
        {
            var eventSenderBaseType = typeof(NetworkedGameEventSender<,>);
            var genericSenderType = eventSenderBaseType.MakeGenericType(deserializerType, eventType);

            var instance = genericSenderType.GetProperty("Instance")!.GetValue(null)!;
            var deserializedEvent = genericSenderType.GetMethod("Deserialize")!.Invoke(instance, [reader]);

            EventRecorder.Instance.Record(deserializedEvent as IGameEvent);
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"Error deserializing game event {eventNameFull} ({typeNameFull}): {e}");
        }
    }
}
