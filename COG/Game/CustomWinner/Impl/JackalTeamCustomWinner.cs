using COG.Game.CustomWinner;
using COG.Utils;
using System.Linq;

namespace COG.Game.CustomWinner.Impl;

public class JackalTeamCustomWinner : IWinnable
{
    public bool CanWin()
    {
        if (PlayerUtils.GetAllAlivePlayers().All(p => p.IsInJackalTeam()))
        {
            CustomWinnerManager.RegisterWinningPlayers(PlayerControl.AllPlayerControls.ToArray().Where(p => p.IsInJackalTeam()));
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
            return true;
        }
        return false;
    }

    public ulong GetWeight() => IWinnable.GetOrder(3);
}