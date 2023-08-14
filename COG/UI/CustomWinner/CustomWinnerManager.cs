using System.Collections.Generic;
using System.Linq;
using COG.Utils;
using UnityEngine;

namespace COG.UI.CustomWinner;

public static class CustomWinnerManager
{
    public static readonly List<ICustomWinner> CustomWinners = new();
    public static Il2CppSystem.Collections.Generic.List<WinningPlayerData> AllWinners { get; } = new();

    public static string WinText { get; private set; } = "";
    public static Color WinColor { get; private set; } = Color.white;

    public static void RegisterCustomWinner(PlayerControl winner)
    {
        AllWinners.Add(new WinningPlayerData(winner.Data));
    }

    public static void RegisterCustomWinners(IEnumerable<PlayerControl> winners)
    {
        Enumerable.ToList(winners).ForEach(w => AllWinners.Add(new WinningPlayerData(w.Data)));
    }

    public static void ResetCustomWinners()
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

    public static void RegisterCustomWinnerInstance(ICustomWinner customWinner)
    {
        CustomWinners.Add(customWinner);
    }

    public static void RegisterCustomWinnersInstances(IEnumerable<ICustomWinner> customWinners)
    {
        ListUtils.ToListCustom(customWinners).ForEach(RegisterCustomWinnerInstance);
    }

    internal static bool CheckEndForCustomWinners()
    {
        // 按照权重从大到小排序
        CustomWinners.Sort((first, second) => second.GetWeight().CompareTo(first.GetWeight()));
        return CustomWinners.All(customWinner => customWinner.CanWin());
    }
}