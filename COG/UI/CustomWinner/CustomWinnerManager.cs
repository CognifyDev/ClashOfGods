using COG.Role;
using COG.Role.Impl.Neutral;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.Core.Tokens;
using CachedPlayer = COG.Utils.CachedPlayer;

namespace COG.UI.CustomWinner;

public static class CustomWinnerManager
{
    public static Il2CppSystem.Collections.Generic.List<WinningPlayerData> AllWinners { get; private set; } = new();
    public static string WinText { get; private set; } = "";
    public static Color WinColor { get; private set; } = Color.white;

    public static void RegisterCustomWinner(PlayerControl winner) => AllWinners.Add(new(winner.Data));

    public static void RegisterCustomWinners(IEnumerable<PlayerControl> winners) => winners.ToList().ForEach(w => AllWinners.Add(new(w.Data)));
    
    public static void ResetCustomWinners() => AllWinners.Clear();

    public static void SetWinText(string text) => WinText = text;
    public static void SetWinColor(Color color) => WinColor = color;
}

public static class CustomGameEndLogicManager
{
    /// <summary>
    /// 检查船员是否可以使游戏结束
    /// </summary>
    /// <returns>是否继续判断下一个逻辑</returns>
    public static bool CheckGameEndForCrewmates()
    {
        bool taskComplete = false;
        PlayerControl.AllPlayerControls.ForEach((Il2CppSystem.Action<PlayerControl>)(p => taskComplete = !p.Data.IsIncomplete));
        if (!taskComplete) 
        {
            if (Utils.PlayerUtils.AllImpostors.Where(pair => pair.Key && !pair.Key.Data.IsDead).Select(pair => pair.Key).ToList().Count == 0)
            {
                CustomWinnerManager.RegisterCustomWinners(Utils.PlayerUtils.AllCremates.Select(p => p.Key));
                CustomWinnerManager.SetWinText("Crewmates win");
                CustomWinnerManager.SetWinColor(Palette.CrewmateBlue);
                GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
                return false;
            }
        }
        else
        {
            CustomWinnerManager.RegisterCustomWinners(Utils.PlayerUtils.AllCremates.Select(p => p.Key));
            CustomWinnerManager.SetWinText("Crewmates win");
            CustomWinnerManager.SetWinColor(Palette.CrewmateBlue);
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByTask, false);
        }
        
        return true;
    }

    /// <summary>
    /// 检查内鬼是否可以使游戏结束
    /// </summary>
    /// <returns>是否继续判断下一个逻辑</returns>
    public static bool CheckGameEndForImpostors()
    {
        var aliveImpostors = Utils.PlayerUtils.AllImpostors.Where(pair => pair.Key && !pair.Key.Data.IsDead).Select(pair=>pair.Key).ToList();
        Utils.GameUtils.Data.Where(pair => pair.Value.CampType == CampType.Impostor).ToList().ForEach(pair => aliveImpostors.Add(pair.Key));
        if (aliveImpostors.Count < Utils.PlayerUtils.GetAllAlivePlayers().Count) return true;
        if (aliveImpostors.Count >= Utils.PlayerUtils.AllCremates.Where(pair => pair.Key && !pair.Key.Data.IsDead).Select(pair => pair.Key).ToList().Count)
        {
            CustomWinnerManager.RegisterCustomWinners(aliveImpostors);
            CustomWinnerManager.SetWinText("Impostors win");
            CustomWinnerManager.SetWinColor(Palette.ImpostorRed);
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
            return false;
        }
        return true;
    }

    public static bool CheckEndForLastPlayer()
    {
        if (Utils.PlayerUtils.GetAllAlivePlayers().Count <= 1)
        {
            var lastPlayer = Utils.PlayerUtils.GetAllAlivePlayers().FirstOrDefault();
            if (!lastPlayer) return true;
            CustomWinnerManager.RegisterCustomWinners(Utils.PlayerUtils.GetAllAlivePlayers());
            CustomWinnerManager.SetWinText($"{lastPlayer!.Data.PlayerName} win");
            CustomWinnerManager.SetWinColor(Color.white);
            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorByKill, false);
            return false;
        }
        return true;
    }

    public static bool CheckEndForJesterExiled()
    {
        var jester = Utils.DeadPlayerManager.DeadPlayers.Where(dp => dp.Role == Jester.Instance && dp.DeathReason == Utils.DeathReason.Exiled).FirstOrDefault();
        if (jester == null) return true;
        GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
        CustomWinnerManager.RegisterCustomWinner(jester.Player);
        return false;
    }
}