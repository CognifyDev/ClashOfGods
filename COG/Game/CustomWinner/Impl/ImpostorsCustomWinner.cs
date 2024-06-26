using System.Linq;
using COG.Config.Impl;
using COG.Role;
using COG.Utils;

namespace COG.Game.CustomWinner.Impl;

public class ImpostorsCustomWinner : IWinnable
{
    public bool CanWin()
    {
        var aliveImpostors = PlayerUtils.AllImpostors.Select(pair => pair.Player).Where
            (p => p && p.IsAlive()).ToList();
        var aliveNeutrals = PlayerUtils.AllNeutrals.Select(pair => pair.Player).Where
            (p => p && p.IsAlive()).ToList();
        GameUtils.PlayerRoleData.Where
                (pair => pair.Role.CampType == CampType.Impostor).ToList()
            .ForEach(pair => aliveImpostors.Add(pair.Player));
        if (aliveImpostors.Count >= PlayerUtils.AllCrewmates
                .Select(pair => pair.Player).Where
                    (p => p && p.IsAlive())
                .ToList().Count
            && aliveNeutrals.Where
                (p => p.GetMainRole().CanKill).ToList().Count <= 0)
        {
            CustomWinnerManager.EndGame(PlayerUtils.AllImpostors.Select(pr => pr.Player), LanguageConfig.Instance.ImpostorsWinText, Palette.ImpostorRed);
            return true;
        }

        var systems = ShipStatus.Instance.Systems;

        {
            if (systems.TryGetValueSafeIl2Cpp(SystemTypes.LifeSupp, out var system))
            {
                LifeSuppSystemType o2Sabo = null!;
                if ((o2Sabo = system.Cast<LifeSuppSystemType>()).Countdown <= 0)
                    if (o2Sabo.Countdown <= 0f)
                    {
                        CustomWinnerManager.EndGame(PlayerUtils.AllImpostors.Select(pr => pr.Player), LanguageConfig.Instance.ImpostorsWinText, Palette.ImpostorRed);
                        return true;
                    }
            }
        }

        {
            SystemTypes[] sabotageTypes =
            {
                SystemTypes.Reactor, SystemTypes.Laboratory, SystemTypes.HeliSabotage
            };
            foreach (var type in sabotageTypes)
                if (systems.TryGetValueSafeIl2Cpp(type, out var system))
                {
                    var sabotage = system.TryCast<ICriticalSabotage>();
                    if (sabotage is null) return false;
                    if (sabotage.Countdown <= 0)
                    {
                        CustomWinnerManager.EndGame(PlayerUtils.AllImpostors.Select(pr => pr.Player), LanguageConfig.Instance.ImpostorsWinText, Palette.ImpostorRed);
                        sabotage.ClearSabotage();
                        return true;
                    }
                }
        }

        return false;
    }

    public ulong GetWeight()
    {
        return IWinnable.GetOrder(1);
    }
}