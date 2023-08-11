using System.Linq;
using COG.Utils;
using UnityEngine;

namespace COG.UI.CustomWinner.Impl;

public class LastPlayerCustomWinner : ICustomWinner
{
    public bool CanWin()
    {
        if (PlayerUtils.GetAllAlivePlayers().Count > 1) return true;
        var lastPlayer = PlayerUtils.GetAllAlivePlayers().FirstOrDefault();
        if (!lastPlayer) return true;
        CustomWinnerManager.RegisterCustomWinners(PlayerUtils.GetAllAlivePlayers());
        CustomWinnerManager.SetWinText($"{lastPlayer!.Data.PlayerName} win");
        CustomWinnerManager.SetWinColor(Color.white);
        GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
        return false;
    }

    public ulong GetWeight()
    {
        return ICustomWinner.GetOrder(3);
    }
}