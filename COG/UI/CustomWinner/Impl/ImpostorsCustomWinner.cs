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
        GameUtils.PlayerRoleData.Where(pair => pair.Role.CampType == CampType.Impostor).ToList()
            .ForEach(pair => aliveImpostors.Add(pair.Player));
        if (aliveImpostors.Count < PlayerUtils.GetAllAlivePlayers().Count) return false;
        if (aliveImpostors.Count >= PlayerUtils.AllCremates
                .Where(pair => pair.Player && !pair.Player.Data.IsDead).Select(pair => pair.Player).ToList().Count)
        {
            CustomWinnerManager.RegisterCustomWinners(PlayerUtils.AllImpostors.Select(pr=>pr.Player));
            CustomWinnerManager.SetWinText("Impostors win");
            CustomWinnerManager.SetWinColor(Palette.ImpostorRed);
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
            return true;
        }

        if (DestroyableSingleton<HeliSabotageSystem>.Instance is HeliSabotageSystem instance) // 等效于检测是否为null并设变量为实例
        {
            if (!instance.IsActive) return false;
            if (instance.Countdown <= 0f) 
            {
                CustomWinnerManager.RegisterCustomWinners(PlayerUtils.AllImpostors.Select(pr => pr.Player));
                CustomWinnerManager.SetWinText("Impostors win");
                CustomWinnerManager.SetWinColor(Palette.ImpostorRed);
                GameManager.Instance.RpcEndGame(GameOverReason.ImpostorBySabotage, false);
                return true;
            }
        }

        return false;
    }

    public ulong GetWeight()
    {
        return IWinnable.GetOrder(1);
    }
}