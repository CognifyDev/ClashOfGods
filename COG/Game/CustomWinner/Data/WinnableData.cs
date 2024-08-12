using System.Collections.Generic;
using COG.Role;
using UnityEngine;

namespace COG.Game.CustomWinner.Data;

public class WinnableData
{
    public bool Winnable { get; set; }
    public string? WinText { get; set; }
    public Color? WinColor { get; set; }

    public GameOverReason GameOverReason { get; set; } = GameOverReason.ImpostorByKill;

    public List<PlayerControl> WinnablePlayers { get; } = new();

    public CampType WinnableCampType { get; set; } = CampType.Unknown;
    
    public static WinnableData Of()
    {
        return new WinnableData();
    }
}