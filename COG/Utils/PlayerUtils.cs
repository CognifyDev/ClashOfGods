using System.Collections.Generic;
using System.Linq;
using COG.States;

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
}