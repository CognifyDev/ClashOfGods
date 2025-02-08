using System.Linq;
using COG.Listener.Event.Impl.Player;
using COG.Patch;
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

                KillAnimationPatch.NextKillerToBeReplaced = toModify.Data;
                @event.Player.MurderPlayer(target, PlayerUtils.SucceededFlags);
                break;
            }
        }
    }
}