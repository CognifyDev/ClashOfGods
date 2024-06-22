using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COG.Game.CustomWinner;

public static class CustomWinnerManager
{
    internal static readonly List<IWinnable> CustomWinners = new();
    public static Il2CppSystem.Collections.Generic.List<WinningPlayerData> AllWinners { get; } = new();

    public static string WinText { get; private set; } = "";
    public static Color WinColor { get; private set; } = Color.white;

    public static void RegisterWinningPlayer(PlayerControl winner)
    {
        AllWinners.Add(new WinningPlayerData(winner.Data));
    }

    public static void UnregisterWinningPlayer(PlayerControl playerControl)
    {
        foreach (var winningPlayerData in AllWinners)
            if (playerControl.Data.PlayerName.Equals(winningPlayerData.PlayerName))
                AllWinners.Remove(winningPlayerData);
    }

    public static void RegisterWinningPlayers(IEnumerable<PlayerControl> winners)
    {
        winners.ToList().ForEach(w => AllWinners.Add(new WinningPlayerData(w.Data)));
    }

    public static void ResetWinningPlayers()
    {
        AllWinners.Clear();
    }

    public static void SetWinText(string text)
    {
        WinText = text;
    }

    public static void SetWinColor(Color color)
    {
        WinColor = color;
    }

    public static void RegisterWinnableInstance(IWinnable customWinner)
    {
        CustomWinners.Add(customWinner);
    }

    public static void RegisterWinnableInstances(IEnumerable<IWinnable> customWinners)
    {
        customWinners.ToList().ForEach(RegisterWinnableInstance);
    }

    internal static bool CheckEndForCustomWinners()
    {
        if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return false;
        // 按照权重从大到小排序
        CustomWinners.Sort((first, second) => second.GetWeight().CompareTo(first.GetWeight()));
        return CustomWinners.All(customWinner => !customWinner.CanWin());
    }

    public static void EndGame(PlayerControl[]? winners = null, string? text = null, Color? color = null, GameOverReason reason = GameOverReason.HumansByVote)
    {
        if (text != null) SetWinText(text);
        if (color != null) SetWinColor(WinColor);
        if (winners != null) RegisterWinningPlayers(winners);
        GameManager.Instance.RpcEndGame(reason, false);
    }
}