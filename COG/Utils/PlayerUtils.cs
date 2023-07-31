using System.Collections.Generic;
using System.Linq;
using COG.Rpc;
using InnerNet;

namespace COG.Utils;

public static class PlayerUtils
{
    public static List<PlayerControl> GetAllPlayers()
    {
        return PlayerControl.AllPlayerControls.ToArray().Where(player => player != null).ToList();
    }

    public static bool IsSamePlayer(this GameData.PlayerInfo info, GameData.PlayerInfo target)
    {
        return info.FriendCode.Equals(target.FriendCode) && info.PlayerName.Equals(target.PlayerName);
    }
    
    public static bool IsSamePlayer(this PlayerControl player, PlayerControl target)
    {
        return player.name.Equals(target.name) && player.FriendCode.Equals(target.FriendCode);
    }

    public static Role.Role? GetRoleInstance(this PlayerControl player) => (from keyValuePair in GameUtils.Data where keyValuePair.Key.IsSamePlayer(player) select keyValuePair.Value).FirstOrDefault();

    public static void SetNamePrivately(PlayerControl target, PlayerControl seer, string name)
    {
        var writer =
            AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, seer.GetClientID());
        writer.Write(name);
        writer.Write(false);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }
    
    public static ClientData? GetClient(this PlayerControl player)
    {
        var client = AmongUsClient.Instance.allClients.ToArray().FirstOrDefault(cd => cd.Character.PlayerId == player.PlayerId);
        return client;
    }

    public static int GetClientID(this PlayerControl player)
    {
        return player.GetClient() == null ? -1 : player.GetClient()!.Id;
    }
}