using COG.Constant;
using COG.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COG.Game.CustomWinner;

public static class CustomWinnerManager
{
    internal static readonly List<IWinnable> CustomWinners = new();
    public static Il2CppSystem.Collections.Generic.List<PlayerControl> AllWinners { get; } = new();

    public static string WinText { get; private set; } = "";
    public static Color WinColor { get; private set; } = Color.white;

    public static void RegisterWinningPlayer(PlayerControl winner)
    {
        AllWinners.Add(winner);
    }

    public static void UnregisterWinningPlayer(PlayerControl playerControl)
    {
        foreach (var winner in AllWinners)
            if (playerControl.IsSamePlayer(winner))
                AllWinners.Remove(winner);
    }

    public static void RegisterWinningPlayers(IEnumerable<PlayerControl> winners)
    {
        winners.ToList().ForEach(AllWinners.Add);
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

    public static void EndGame(IEnumerable<PlayerControl> winners, string? text = null, Color? color = null, bool ignoreDebugMode = false)
    {
        if (!ignoreDebugMode && GlobalCustomOptionConstant.DebugMode!.GetBool()) return;
        if (text != null) SetWinText(text);
        if (color != null) SetWinColor(WinColor);
        RegisterWinningPlayers(winners);
        var num = (GameOverReason)winners.Where(p => p).Select(p => (int)p.PlayerId).Sum();
        GameOverReason modCustom = (GameOverReason)4673347; // String "COG " (4 characters) to byte array to integer
        GameManager.Instance.RpcEndGame(modCustom | num, false);
    }
}