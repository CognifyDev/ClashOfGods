using System.Linq;
using System.Reflection;
using COG.Game.Events;
using COG.Listener.Event.Impl.Player;
using COG.Role;
using COG.Rpc;
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
                        option?.UpdateSelection((int)selection);
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

                killer.KillWithoutDeadBody(target, showAnimationToEverybody, anonymousKiller);
                break;
            }

            case KnownRpc.Revive:
            {
                reader.ReadNetObject<PlayerControl>().Revive();
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
                    playerData.Tags.Remove(tag.Replace(PlayerUtils.DeleteTagPrefix, ""));
                    break;
                }

                playerData.Tags.Add(tag);
                break;
            }

            case KnownRpc.SyncRoleGameData:
            {
                var roleId = -1;
                CustomRole role;

                try
                {
                    roleId = reader.ReadPackedInt32();
                    role = CustomRoleManager.GetManager().GetRoleById(roleId) ?? throw new System.Exception();
                }
                catch
                {
                    Main.Logger.LogError($"Got invalid {nameof(roleId)} while synchronizing role data: {roleId}");
                    return;
                }

                Main.Logger.LogMessage($"Syncing game data for {role.Name}...");
                role.OnRoleGameDataGettingSynchronized(reader);
                break;
            }

            case KnownRpc.AdvancedMurder:
            {
                @event.Player.MurderAdvanced(AdvancedKillOptions.Deserialize(reader));
                break;
            }

            case KnownRpc.SyncGameEvent:
            {
                var eventNameFull = reader.ReadString();
                var typeNameFull = reader.ReadString();

                var eventType = Main.Assembly.GetTypes().FirstOrDefault(t => t.FullName == eventNameFull);
                var deserializerType = Main.Assembly.GetTypes().FirstOrDefault(t => t.FullName == typeNameFull);

                if (eventType == null || deserializerType == null)
                {
                    Main.Logger.LogError($"Unsupported event type: {eventNameFull} ({typeNameFull})");
                    break;
                }

                try
                {
                    var eventSenderBaseType = typeof(NetworkedGameEventSender<,>);
                    var genericSenderType = eventSenderBaseType.MakeGenericType(deserializerType, eventType);

                    var instance = genericSenderType.GetProperty(nameof(NetworkedGameEventSender<,>.Instance))!.GetValue(null)!;
                    var deserializedEvent = genericSenderType.GetMethod(nameof(NetworkedGameEventSender<,>.Deserialize))!.Invoke(instance, [reader]);

                    EventRecorder.Instance.Record(deserializedEvent as IGameEvent);
                }
                catch (System.Exception e)
                {
                    Main.Logger.LogError($"Error deserializing game event {eventNameFull} ({typeNameFull}): {e}");
                }

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