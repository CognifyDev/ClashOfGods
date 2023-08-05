using System.Collections.Generic;
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
        // /rpc <CallId> <Byte...>
        try
        {
            var callId = byte.Parse(args[0]);
            List<byte> bytes = new();
            for (var i = 1; i < args.Length; i++) bytes.Add(byte.Parse(args[i]));

            foreach (var playerControl in PlayerUtils.GetAllPlayers())
            {
                var writer = AmongUsClient.Instance.StartRpcImmediately(player.NetId, callId, SendOption.Reliable,
                    playerControl.GetClientID());
                writer.Write(bytes.ToArray());
                AmongUsClient.Instance.FinishRpcImmediately(writer);
                GameUtils.SendGameMessage("成功向" + playerControl.name + "发送Rpc " + callId + " " +
                                          bytes.ToArray().AsString());
            }
        }
        catch
        {
            GameUtils.SendGameMessage("发送失败，检查数据格式");
        }
    }
}