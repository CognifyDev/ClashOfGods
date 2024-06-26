using System.Linq;
using COG.Config.Impl;
using COG.Utils;

namespace COG.Game.CustomWinner.Impl;

public class LastPlayerCustomWinner : IWinnable
{
    public bool CanWin()
    {
        if (PlayerUtils.GetAllAlivePlayers().Count != 1) return false;
        var lastPlayer = PlayerUtils.GetAllAlivePlayers().FirstOrDefault();
        if (!lastPlayer) return false;
        CustomWinnerManager.EndGame(PlayerUtils.GetAllAlivePlayers(),
            LanguageConfig.Instance.NeutralsWinText.CustomFormat(lastPlayer!.Data.PlayerName),
            lastPlayer.GetMainRole()!.Color);
        return true;
    }

    public ulong GetWeight()
    {
        return IWinnable.GetOrder(4);
    }
}