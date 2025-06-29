using System;
using System.Linq;
using System.Reflection;
using COG.Listener.Event.Impl.Player;
using COG.Patch;
using COG.Role;
using COG.Rpc;
using COG.States;
using COG.UI.CustomOption;
using COG.Utils;
using InnerNet;

namespace COG.Listener.Impl;

public class RpcListener : IListener
{
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
            
            case KnownRpc.KillPlayerCompletely:
            {
                var killer = reader.ReadNetObject<PlayerControl>();
                var target = reader.ReadNetObject<PlayerControl>();
                var showAnimationToEverybody = reader.ReadBoolean();
                var anonymousKiller = reader.ReadBoolean();

                killer.KillPlayerCompletely(target, showAnimationToEverybody, anonymousKiller);
                break;
            }

            case KnownRpc.MurderAndModifyKillAnimation:
            {
                var target = reader.ReadNetObject<PlayerControl>();
                var toModify = reader.ReadNetObject<PlayerControl>();
                var modifyDeathData = reader.ReadBoolean();

                KillAnimationPatch.NextKillerToBeReplaced = toModify.Data;
                if (modifyDeathData)
                    _ = new DeadPlayer(DateTime.Now, CustomDeathReason.Default, target.Data, toModify.Data);
                @event.Player.MurderPlayer(target, PlayerUtils.SucceededFlags);
                break;
            }

            case KnownRpc.ShareRoles:
            {
                if (AmongUsClient.Instance.AmHost) return;
                // 清除原列表，防止干扰
                GameUtils.PlayerData.Clear();
                // 开始读入数据
                Main.Logger.LogDebug("Received role assignment data, applying...");

                var count = reader.ReadPackedInt32();

                for (var i = 0; i < count; i++)
                {
                    var bytes = reader.ReadBytesAndSize().ToArray();
                    var data = bytes.DeserializeToData<SerializablePlayerData>().AsPlayerData();
                    GameUtils.PlayerData.Add(data);
                    data.Player.SetCustomRole(data.MainRole, data.SubRoles);
                }
                
                foreach (var playerRole in GameUtils.PlayerData)
                    Main.Logger.LogInfo($"{playerRole.Player.Data.PlayerName}({playerRole.Player.Data.FriendCode})" +
                                        $" => {playerRole.MainRole.GetNormalName()}{playerRole.SubRoles.AsString()}");

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

                var hasMark = role.GetType().GetMethod(nameof(CustomRole.OnRpcReceived), BindingFlags.Public)!
                        .GetCustomAttribute<OnlyLocalPlayerWithThisRoleInvokableAttribute>() != null;
                if (!hasMark || (hasMark && role.IsLocalPlayerRole()))
                    role.OnRpcReceived(@event.Player, callId, reader);
                break;
            }
        }
    }
}