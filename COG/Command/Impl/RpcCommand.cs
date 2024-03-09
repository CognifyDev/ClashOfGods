using System.Text;
using COG.Utils;

namespace COG.Command.Impl;

public class RpcCommand : Command
{
    public RpcCommand() : base("rpc")
    {
        HostOnly = true;
    }

    public override void OnExecute(PlayerControl player, string[] args)
    {
        // /rpc <CallId> <string>
        try
        {
            var callId = byte.Parse(args[0]);
            var sb = new StringBuilder();
            for (var i = 1; i < args.Length; i++)
            {
                sb.Append(args[i]);
                if (i < args.Length - 1) sb.Append(' ');
            }

            foreach (var playerControl in PlayerUtils.GetAllPlayers())
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, callId, SendOption.Reliable,
                    playerControl.GetClientID());
                writer.Write(sb.ToString());
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                GameUtils.SendGameMessage("成功向" + playerControl.name + "发送Rpc " + callId + " " + sb);
            }
        }
        catch
        {
            GameUtils.SendGameMessage("发送失败，检查数据格式");
        }
    }
}