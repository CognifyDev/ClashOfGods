using COG.Game.Events;
using COG.Listener.Event.Impl.Player;
using COG.Patch;
using COG.Role;
using COG.Rpc;
using COG.States;
using COG.UI.CustomOption;
using COG.Utils;
using InnerNet;
using System;
using System.Linq;
using System.Reflection;

namespace COG.Listener.Impl;

public class RpcListener : IListener
{
    private string? _murderExtraMessage = null;

    [EventHandler(EventHandlerType.Prefix)]
    public void BeforeRpcBeingProceeded(PlayerHandleRpcEvent @event)
    {
        var reader = MessageReader.Get(@event.Reader);
        var rpc = @event.CallId;
        reader.ReadNetObject<PlayerControl>();
        if (rpc == (byte)RpcCalls.CheckMurder && reader.BytesRemaining > 0) // with extra data
        {
            // CheckMurder RPC will be sent to every client, but whether to perform depends if the client is the host
            // Killer ---Send CheckMurder--> Each client --> Perform
            // MuderPlayer will be sent even if the kill failed
            // Kinda complex...
            _murderExtraMessage = reader.ReadString();
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerMurder(PlayerMurderEvent @event)
    {
        if (@event.MurderResult!.Value.HasFlag(MurderResultFlags.Succeeded) && _murderExtraMessage != null)
            GameEventPatch.ExtraMessage = _murderExtraMessage;
        
        _murderExtraMessage = null;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void AfterRpcReceived(PlayerHandleRpcEvent @event)
    {
        var callId = @event.CallId;
        var reader = @event.Reader;
        var knownRpc = (KnownRpc)callId;

        switch (knownRpc)
        {
            case KnownRpc.UpdateOption:
            {
                var id = reader.ReadPackedInt32();
                var selection = reader.ReadPackedInt32();

                var option = CustomOption.Options.FirstOrDefault(o => o.Id == id);
                if (option == null) return;

                option.Selection = selection;
                break;
            }
            
            case KnownRpc.ShareOptions:
            {
                var count = reader.ReadPackedUInt32();
                try
                {
                    for (var i = 0; i < count; i++)
                    {
                        var optionId = reader.ReadPackedUInt32();
                        var selection = reader.ReadPackedUInt32();
                        var option = CustomOption.Options.FirstOrDefault(option => option.Id == (int)optionId);
                        option?.UpdateSelection((int) selection);
                    }
                }
                catch (System.Exception e)
                {
                    Main.Logger.LogError("Error while deserializing options: " + e.Message);
                }
                
                break;
            }
            
            case KnownRpc.SetRole:
            {
                var playerId = reader.ReadByte();
                var roleId = reader.ReadPackedInt32();
                PlayerUtils.GetPlayerById(playerId)!.SetCustomRole(CustomRoleManager.GetManager().GetRoleById(roleId)!);
                break;
            }
            
            case KnownRpc.NotifySettingChange:
            {
                var id = reader.ReadPackedInt32();
                var text = reader.ReadString();
                HudManager.Instance.Notifier.SettingsChangeMessageLogic((StringNames)id, text, true);
                break;    
            }
            
            case KnownRpc.KillWithoutDeadBody:
            {
                var killer = reader.ReadNetObject<PlayerControl>();
                var target = reader.ReadNetObject<PlayerControl>();
                var showAnimationToEverybody = reader.ReadBoolean();
                var anonymousKiller = reader.ReadBoolean();

                killer.RpcKillWithoutDeadBody(target, showAnimationToEverybody, anonymousKiller);
                break;
            }

            case KnownRpc.MurderAndModifyKillAnimation:
            {
                var target = reader.ReadNetObject<PlayerControl>();
                var toModify = reader.ReadNetObject<PlayerControl>();
                var modifyDeathData = reader.ReadBoolean();

                KillAnimationPatch.NextKillerToBeReplaced = toModify.Data;
                if (modifyDeathData)
                    DeadPlayer.Create(DateTime.Now, CustomDeathReason.Default, target.Data, toModify.Data);
                @event.Player.MurderPlayer(target, PlayerUtils.SucceededFlags);
                break;
            }

            case KnownRpc.Revive:
            {
                // 从Rpc中读入PlayerControl
                var target = reader.ReadNetObject<PlayerControl>();
                
                // 复活目标玩家
                target.Revive();
                break;
            }

            case KnownRpc.HideDeadBody:
            {
                if (!GameStates.InRealGame) return;
                var pid = reader.ReadByte();
                var body = Object.FindObjectsOfType<DeadBody>().ToList().FirstOrDefault(b => b.ParentId == pid);
                if (!body) return;
                body!.gameObject.SetActive(false);
                break;
            }

            case KnownRpc.Mark:
            {
                var target = reader.ReadNetObject<PlayerControl>();
                var tag = reader.ReadString();
                var playerData = target.GetPlayerData();

                if (tag.StartsWith(PlayerUtils.DeleteTagPrefix))
                {
                    playerData?.Tags.Remove(tag.Replace(PlayerUtils.DeleteTagPrefix, ""));
                    break;
                }
                
                playerData?.Tags.Add(tag);
                break;
            }

            case KnownRpc.SyncRoleGameData:
            {
                int roleId = -1;
                CustomRole role = null!;

                try
                {
                    roleId = reader.ReadPackedInt32();
                    role = CustomRoleManager.GetManager().GetRoleById(roleId) ?? throw new();
                }
                catch
                {
                    Main.Logger.LogError($"Get invalid {nameof(roleId)} while synchronizing role data: {roleId}");
                    return;
                }

                Main.Logger.LogMessage($"Syncing game data for {role.Name}...");
                role.OnRoleGameDataGettingSynchronized(reader);
                break;
            }

            case KnownRpc.DieWithoutAnimationAndBody:
            {
                var reason = (CustomDeathReason)reader.ReadPackedInt32();
                @event.Player.DieWithoutAnimationAndBody(reason);
                break;
            }
        }

        IRpcHandler.Handlers.ForEach(h =>
        {
            dynamic handler = h;
            if (handler.CallId != callId) return;
            h.OnReceive(reader);
        });

        CustomRoleManager.GetManager().GetRoles().ForEach(cr => cr.OnRpcReceived(@event.Player, callId, reader));
    }
}