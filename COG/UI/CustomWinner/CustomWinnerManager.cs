using COG.UI.CustomButtons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace COG.UI.CustomWinner
{
    public static class CustomWinnerManager
    {
        public static Il2CppSystem.Collections.Generic.List<WinningPlayerData> AllWinners { get => TempData.winners; }
        public static string WinText { get; set; } = "";
        public static Color WinColor { get; set; } = Color.white;

        public static void RegisterCustomWinner(PlayerControl winner) => AllWinners.Add(new(winner.Data));
        public static void RegisterCustomWinner(GameData.PlayerInfo winner) => AllWinners.Add(new(winner));

        public static void RegisterCustomWinners(IEnumerable<PlayerControl> winners) => winners.ToList().ForEach(w => AllWinners.Add(new(w.Data)));
        public static void RegisterCustomWinners(IEnumerable<GameData.PlayerInfo> winners) => winners.ToList().ForEach(w => AllWinners.Add(new(w)));

        public static void SetWinText(string text) => WinText = text;
        public static void SetWinColor(Color color) => WinColor = color;

        public static void SetTextAndColor(string text, Color color)
        {
            SetWinText(text);
            SetWinColor(color);
        }

        public static void ResetWinners() => TempData.winners.Clear();
    }
}
