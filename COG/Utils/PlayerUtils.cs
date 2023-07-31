using System.Collections.Generic;
using System.Linq;
using InnerNet;
using GameStates = COG.States.GameStates;

namespace COG.Utils;

public static class PlayerUtils
{
    public static List<PlayerControl> GetAllPlayers() =>
        PlayerControl.AllPlayerControls.ToArray().Where(player => player != null).ToList();

    public static List<PlayerControl> GetAllAlivePlayers() => 
        PlayerControl.AllPlayerControls.ToArray().Where(player => player != null && player.IsAlive()).ToList();

    public static bool IsSamePlayer(this GameData.PlayerInfo info, GameData.PlayerInfo target) =>
        info.FriendCode.Equals(target.FriendCode) && info.PlayerName.Equals(target.PlayerName);
    
    public static bool IsSamePlayer(this PlayerControl player, PlayerControl target) =>
        player.name.Equals(target.name) && player.FriendCode.Equals(target.FriendCode);

    public static Role.Role? GetRoleInstance(this PlayerControl player) => 
        (from keyValuePair in GameUtils.Data where keyValuePair.Key.IsSamePlayer(player) select keyValuePair.Value).FirstOrDefault();

    public static void SetNamePrivately(PlayerControl target, PlayerControl seer, string name)
    {
        var writer =
            AmongUsClient.Instance.StartRpcImmediately(target.NetId, (byte)RpcCalls.SetName, SendOption.Reliable, seer.GetClientID());
        writer.Write(name);
        writer.Write(false);
        AmongUsClient.Instance.FinishRpcImmediately(writer);
    }

    public static PlayerControl? GetPlayerById(byte playerId)
    {
        foreach (var playerControl in GetAllPlayers())
        {
            if (playerControl.PlayerId == playerId) return playerControl;
        }

        return null;
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

    /// <summary>
    /// 检测玩家是否存活
    /// </summary>
    /// <param name="player">玩家实例</param>
    /// <returns></returns>
    public static bool IsAlive(this PlayerControl player)
    {
        if (GameStates.IsLobby) return true;
        if (player == null) return false;
        return !player.Data.IsDead;
    }
}