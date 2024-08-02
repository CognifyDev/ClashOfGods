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
        if (taskComplete ||
            PlayerUtils.AllImpostors.Where(pair => pair.Player && !pair.Player.Data.IsDead)
                .Select(pair => pair.Player).ToList().Count == 0)
        {
            CustomWinnerManager.EndGame(PlayerUtils.AllCrewmates.Select(p => p.Player),
                LanguageConfig.Instance.CrewmatesWinText,
                Palette.CrewmateBlue);
            return true;
        }

        return false;
    }

    public ulong GetWeight()
    {
        return IWinnable.GetOrder(2);
    }
}