using System.Linq;
using COG.Role;
using COG.Utils;

namespace COG.UI.CustomWinner.Impl;

public class ImpostorsCustomWinner : ICustomWinner
{
    public bool CanWin()
    {
        var aliveImpostors = Enumerable.ToList(PlayerUtils.AllImpostors.Where(pair => pair.Key && !pair.Key.Data.IsDead)
            .Select(pair => pair.Key));
        Enumerable.ToList(GameUtils.Data.Where(pair => pair.Value.CampType == CampType.Impostor))
            .ForEach(pair => aliveImpostors.Add(pair.Key));
        if (aliveImpostors.Count < PlayerUtils.GetAllAlivePlayers().Count) return true;
        if (aliveImpostors.Count >= Enumerable.ToList(PlayerUtils.AllCremates
                .Where(pair => pair.Key && !pair.Key.Data.IsDead).Select(pair => pair.Key)).Count)
        {
            CustomWinnerManager.RegisterCustomWinners(aliveImpostors);
            CustomWinnerManager.SetWinText("Impostors win");
            CustomWinnerManager.SetWinColor(Palette.ImpostorRed);
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
            return false;
        }

        return true;
    }

    public ulong GetWeight()
    {
        return ICustomWinner.GetOrder(1);
    }
}