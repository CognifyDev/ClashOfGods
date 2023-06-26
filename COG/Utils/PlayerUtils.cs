using System.Collections.Generic;
using System.Linq;
using COG.States;

namespace COG.Utils;

public static class PlayerUtils
{
    public static IEnumerable<PlayerControl> GetAllPlayers()
    {
        return PlayerControl.AllPlayerControls.ToArray().Where(player => player != null);
    }
}