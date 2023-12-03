using System.Linq;
using COG.Role;
using COG.Utils;

namespace COG.UI.CustomWinner.Impl;

public class ImpostorsCustomWinner : IWinnable
{
    public bool CanWin()
    {
        var aliveImpostors = PlayerUtils.AllImpostors.Where(pair => pair.Player && !pair.Player.Data.IsDead)
            .Select(pair => pair.Player).ToList();
        GameUtils.Data.Where(pair => pair.Role.CampType == CampType.Impostor).ToList()
            .ForEach(pair => aliveImpostors.Add(pair.Player));
        if (aliveImpostors.Count < PlayerUtils.GetAllAlivePlayers().Count) return false;
        if (aliveImpostors.Count >= PlayerUtils.AllCremates
                .Where(pair => pair.Player && !pair.Player.Data.IsDead).Select(pair => pair.Player).ToList().Count)
        {
            CustomWinnerManager.RegisterCustomWinners(aliveImpostors);
            CustomWinnerManager.SetWinText("Impostors win");
            CustomWinnerManager.SetWinColor(Palette.ImpostorRed);
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
            return true;
        }

        return false;
    }

    public ulong GetWeight()
    {
        return IWinnable.GetOrder(1);
    }
}