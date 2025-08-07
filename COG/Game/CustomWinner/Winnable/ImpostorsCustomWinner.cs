using System.Linq;
using COG.Config.Impl;
using COG.Game.CustomWinner.Data;
using COG.Role;
using COG.Utils;

namespace COG.Game.CustomWinner.Winnable;

public class ImpostorsCustomWinner : IWinnable
{
    public void CheckWin(WinnableData data)
    {
        var aliveImpostors = PlayerUtils.AllImpostors.Select(pair => pair.Player).Where
            (p => p.IsAlive()).ToList();
        var aliveNeutrals = PlayerUtils.AllNeutrals.Select(pair => pair.Player).Where
            (p => p.IsAlive()).ToList();
        var aliveCrewmates = PlayerUtils.AllCrewmates.Select(pair => pair.Player).Where
            (p => p.IsAlive()).ToList();

        if (aliveImpostors.Count >= aliveCrewmates.Count &&
            aliveNeutrals.Where(p => p.GetMainRole().CanKill).ToList().Count <= 0)
            EndGame(data, true);

        if (CheckGameEndBySabotage()) EndGame(data, false);
    }

    public uint GetWeight()
    {
        return IWinnable.GetOrder(1);
    }

    private static void EndGame(WinnableData data, bool kill)
    {
        data.GameOverReason = kill ? GameOverReason.ImpostorsByKill : GameOverReason.ImpostorsBySabotage;
        data.WinnableCampType = CampType.Impostor;
        data.WinnablePlayers.AddRange(PlayerUtils.AllImpostors.Select(pr => pr.Data));
        data.WinText = LanguageConfig.Instance.ImpostorsWinText;
        data.WinColor = Palette.ImpostorRed;
        data.Winnable = true;
    }

    private static bool CheckGameEndBySabotage()
    {
        if (ShipStatus.Instance.Systems == null) return false;

        var systems = ShipStatus.Instance.Systems;
        LifeSuppSystemType? lifeSupp;
        if (systems.ContainsKey(SystemTypes.LifeSupp) &&
            (lifeSupp = systems[SystemTypes.LifeSupp].TryCast<LifeSuppSystemType>()) != null &&
            lifeSupp.Countdown < 0f)
        {
            lifeSupp.Countdown = 10000f;
            return true;
        }

        ISystemType? sys = null;
        if (systems.ContainsKey(SystemTypes.Reactor)) sys = systems[SystemTypes.Reactor];
        else if (systems.ContainsKey(SystemTypes.Laboratory)) sys = systems[SystemTypes.Laboratory];
        else if (systems.ContainsKey(SystemTypes.HeliSabotage)) sys = systems[SystemTypes.HeliSabotage];

        ICriticalSabotage? critical;
        if (sys == null ||
            (critical = sys.TryCast<ICriticalSabotage>()) == null ||
            !(critical.Countdown < 0f)) return false;
        critical.ClearSabotage();
        return true;
    }
}