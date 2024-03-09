using System.Linq;
using COG.Config.Impl;
using COG.Role;
using COG.Utils;

namespace COG.Game.CustomWinner.Impl;

public class ImpostorsCustomWinner : IWinnable
{
    public bool CanWin()
    {
        var aliveImpostors = PlayerUtils.AllImpostors.Where(pair => pair.Player && !pair.Player.Data.IsDead)
            .Select(pair => pair.Player).ToList();
        var aliveNeutrals = PlayerUtils.AllNeutrals.Where(pair => pair.Player && !pair.Player.Data.IsDead)
            .ToList();
        GameUtils.PlayerRoleData.Where(pair => pair.Role.CampType == CampType.Impostor).ToList()
            .ForEach(pair => aliveImpostors.Add(pair.Player));
        if (aliveImpostors.Count >= PlayerUtils.AllCremates
                .Where(pair => pair.Player && !pair.Player.Data.IsDead).Select(pair => pair.Player).ToList().Count
            && aliveNeutrals.Select(p => p.Role.CanKill).ToList().Count <= 0)
        {
            CustomWinnerManager.RegisterCustomWinners(PlayerUtils.AllImpostors.Select(pr => pr.Player));
            CustomWinnerManager.SetWinText(LanguageConfig.Instance.ImpostorsWinText);
            CustomWinnerManager.SetWinColor(Palette.ImpostorRed);
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
            return true;
        }

        if (DestroyableSingleton<HeliSabotageSystem>.Instance is { } instance) // 等效于检测是否为null并设变量为实例
        {
            if (!instance.IsActive) return false;
            if (instance.Countdown <= 0f)
            {
                CustomWinnerManager.RegisterCustomWinners(PlayerUtils.AllImpostors.Select(pr => pr.Player));
                CustomWinnerManager.SetWinText(LanguageConfig.Instance.ImpostorsWinText);
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