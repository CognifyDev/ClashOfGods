using System.Linq;
using COG.Utils;
using UnityEngine;

namespace COG.UI.CustomWinner.Impl;

public class LastPlayerCustomWinner : IWinnable
{
    public bool CanWin()
    {
        if (PlayerUtils.GetAllAlivePlayers().Count > 1) return false;
        var lastPlayer = PlayerUtils.GetAllAlivePlayers().FirstOrDefault();
        if (!lastPlayer) return false;
        CustomWinnerManager.RegisterCustomWinners(PlayerUtils.GetAllAlivePlayers());
        CustomWinnerManager.SetWinText($"{lastPlayer!.Data.PlayerName} win");
        CustomWinnerManager.SetWinColor(Color.white);
        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
        return true;
    }

    public ulong GetWeight()
    {
        return IWinnable.GetOrder(3);
    }
}