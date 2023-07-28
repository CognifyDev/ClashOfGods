using System.Collections.Generic;
using System.Linq;

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
}