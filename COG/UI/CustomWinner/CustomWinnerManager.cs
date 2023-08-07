using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace COG.UI.CustomWinner;

public static class CustomWinnerManager
{
    public static Il2CppSystem.Collections.Generic.List<PlayerControl> AllWinners => new();
    public static string WinText { get; private set; } = "";
    public static Color WinColor { get; private set; } = Color.white;

    public static void RegisterCustomWinner(PlayerControl winner) => AllWinners.Add(winner);

    public static void RegisterCustomWinners(IEnumerable<PlayerControl> winners) => winners.ToList().ForEach(w => AllWinners.Add(w));
    
    public static void ResetCustomWinners() => AllWinners.Clear();

    public static void SetWinText(string text) => WinText = text;
    public static void SetWinColor(Color color) => WinColor = color;
}