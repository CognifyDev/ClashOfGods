using System.Linq;
using COG.Listener.Event.Impl.Player;
using COG.Role;
using COG.Rpc;
using COG.UI.CustomOption;
using COG.Utils;

namespace COG.Listener.Impl;

public class RpcListener : IListener
{
    [EventHandler(EventHandlerType.Postfix)]
    public void AfterRPCReceived(PlayerHandleRpcEvent @event)
    {
        var callId = @event.CallId;
        var reader = @event.Reader;
        if (AmongUsClient.Instance.AmHost) return; // 是房主就返回
        var knownRpc = (KnownRpc)callId;

        switch (knownRpc)
        {
            case KnownRpc.UpdateOption:
            {
                var id = reader.ReadPackedInt32();
                var selection = reader.ReadPackedInt32();

                var option = CustomOption.Options.FirstOrDefault(o => o != null && o.Id == id);
                if (option == null) return;

                option.Selection = selection;
                break;
            }
            case KnownRpc.ShareOptions:
            {
                var originalString = reader.ReadString();
                Main.Logger.LogInfo("Received options string => " + originalString);
                foreach (var s in originalString.Split(","))
                {
                    var contexts = s.Split("|");
                    var id = int.Parse(contexts[0]);
                    var selection = int.Parse(contexts[1]);

                    for (var i = 0; i < CustomOption.Options.Count; i++)
                    {
                        var option = CustomOption.Options[i];
                        if (option == null) continue;
                        if (option.Id != id) continue;
                        Main.Logger.LogInfo(
                            $"Changed {option.Name()}({option.Id})'s selection to {selection}(before: {option.Selection})");
                        option.Selection = selection;
                        CustomOption.Options[i] = option;
                    }
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
            case KnownRpc.Handshake:
            {
                var versionStr = reader.ReadString();
                var commitTime = reader.ReadString();
                HandshakeManager.Instance.AddInfo(@event.Player, versionStr, commitTime);
                HandshakeManager.Instance.CheckPlayersAndDisplay();
                break;
            }
        }
    }
}