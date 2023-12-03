using System.Linq;
using COG.Utils;

namespace COG.UI.CustomWinner.Impl;

public class CrewmatesCustomWinner : IWinnable
{
    public bool CanWin()
    {
        var taskComplete = false;
        foreach (var player in PlayerUtils.GetAllPlayers()) taskComplete = !player.Data.IsIncomplete;
        if (!taskComplete)
        {
            if (PlayerUtils.AllImpostors.Where(pair => pair.Player && !pair.Player.Data.IsDead)
                    .Select(pair => pair.Player).ToList().Count == 0)
            {
                CustomWinnerManager.RegisterCustomWinners(PlayerUtils.AllCremates.Select(p => p.Player));
                CustomWinnerManager.SetWinText("Crewmates win");
                CustomWinnerManager.SetWinColor(Palette.CrewmateBlue);
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
        }
        else
        {
            CustomWinnerManager.RegisterCustomWinners(PlayerUtils.AllCremates.Select(p => p.Player));
            CustomWinnerManager.SetWinText("Crewmates win");
            CustomWinnerManager.SetWinColor(Palette.CrewmateBlue);
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByTask, false);
        }

        return false;
    }

    public ulong GetWeight()
    {
        return IWinnable.GetOrder(2);
    }
}