using System.Linq;
using COG.Utils;

namespace COG.UI.CustomWinner.Impl;

public class CrewmatesCustomWinner : ICustomWinner
{
    public bool CanWin()
    {
        var taskComplete = false;
        foreach (var player in PlayerUtils.GetAllPlayers()) taskComplete = !player.Data.IsIncomplete;
        if (!taskComplete)
        {
            if (Enumerable.ToList(PlayerUtils.AllImpostors.Where(pair => pair.Key && !pair.Key.Data.IsDead)
                    .Select(pair => pair.Key)).Count == 0)
            {
                CustomWinnerManager.RegisterCustomWinners(PlayerUtils.AllCremates.Select(p => p.Key));
                CustomWinnerManager.SetWinText("Crewmates win");
                CustomWinnerManager.SetWinColor(Palette.CrewmateBlue);
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return false;
            }
        }
        else
        {
            CustomWinnerManager.RegisterCustomWinners(PlayerUtils.AllCremates.Select(p => p.Key));
            CustomWinnerManager.SetWinText("Crewmates win");
            CustomWinnerManager.SetWinColor(Palette.CrewmateBlue);
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByTask, false);
        }

        return true;
    }

    public ulong GetWeight()
    {
        return ICustomWinner.GetOrder(2);
    }
}