using System.Linq;
using COG.Config.Impl;
using COG.Utils;

namespace COG.Game.CustomWinner.Impl;

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
                CustomWinnerManager.RegisterWinningPlayers(PlayerUtils.AllCremates.Select(p => p.Player));
                CustomWinnerManager.SetWinText(LanguageConfig.Instance.CrewmatesWinText);
                CustomWinnerManager.SetWinColor(Palette.CrewmateBlue);
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return true;
            }
        }
        else
        {
            CustomWinnerManager.RegisterWinningPlayers(PlayerUtils.AllCremates.Select(p => p.Player));
            CustomWinnerManager.SetWinText(LanguageConfig.Instance.CrewmatesWinText);
            CustomWinnerManager.SetWinColor(Palette.CrewmateBlue);
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByTask, false);
            return true;
        }

        return false;
    }

    public ulong GetWeight()
    {
        return IWinnable.GetOrder(2);
    }
}