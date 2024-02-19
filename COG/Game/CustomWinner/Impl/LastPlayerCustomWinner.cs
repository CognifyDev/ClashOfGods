using System.Linq;
using COG.Config.Impl;
using COG.Utils;

namespace COG.Game.CustomWinner.Impl;

public class LastPlayerCustomWinner : IWinnable
{
    public bool CanWin()
    {
        if (PlayerUtils.GetAllAlivePlayers().Count > 1) return false;
        var lastPlayer = PlayerUtils.GetAllAlivePlayers().FirstOrDefault();
        if (!lastPlayer) return false;
        CustomWinnerManager.RegisterCustomWinners(PlayerUtils.GetAllAlivePlayers());
        CustomWinnerManager.SetWinText(LanguageConfig.Instance.NeutralsWinText.CustomFormat(lastPlayer!.Data.PlayerName));
        CustomWinnerManager.SetWinColor(lastPlayer.GetRoleInstance()!.Color);
        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
        return true;
    }

    public ulong GetWeight() => IWinnable.GetOrder(4);
}