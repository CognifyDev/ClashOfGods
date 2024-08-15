using System.Linq;
using COG.Config.Impl;
using COG.Game.CustomWinner.Data;
using COG.Utils;

namespace COG.Game.CustomWinner.Winnable;

public class LastPlayerCustomWinner : IWinnable
{
    public void CheckWin(WinnableData data)
    {
        if (PlayerUtils.GetAllAlivePlayers().Count != 1) return;
        var lastPlayer = PlayerUtils.GetAllAlivePlayers().FirstOrDefault();
        if (!lastPlayer) return;

        data.WinnableCampType = lastPlayer!.GetMainRole().CampType;
        data.WinnablePlayers.AddRange(PlayerUtils.GetAllAlivePlayers().Select(p => p.Data));
        data.WinText = LanguageConfig.Instance.NeutralsWinText.CustomFormat(lastPlayer!.Data.PlayerName);
        data.WinColor = lastPlayer.GetMainRole().Color;
        data.Winnable = true;
    }

    public uint GetWeight()
    {
        return IWinnable.GetOrder(4);
    }
}