using System;
using System.Collections.Generic;
using System.Linq;
using COG.Role;
using COG.Utils;
using UnityEngine;

namespace COG.Game.CustomWinner.Data;

public class WinnableData
{
    public bool Winnable { get; set; }
    public string? WinText { get; set; }
    public Color? WinColor { get; set; }

    public GameOverReason GameOverReason { get; set; } = GameOverReason.ImpostorByKill;

    public List<NetworkedPlayerInfo> WinnablePlayers { get; } = new();

    public List<CachedPlayerData> GetWinnablePlayersAsCachedPlayerData()
    {
        return new List<CachedPlayerData>(WinnablePlayers.Select(info => new CachedPlayerData(info)).ToList());
    }

    public CampType WinnableCampType { get; set; } = CampType.Unknown;
    
    public static WinnableData Of()
    {
        return new WinnableData();
    }
}

[Serializable]
public class SerializableWinnableData
{
    private SerializableWinnableData(bool winnable, string? winText, float[] color,
        int gameOverReason, byte[] winnablePlayers, int winnableCampType)
    {
        Winnable = winnable;
        WinText = winText;
        Color = color;
        GameOverReason = gameOverReason;
        WinnablePlayers = winnablePlayers;
        WinnableCampType = winnableCampType;
    }

    public bool Winnable { get; }
    public string? WinText { get; }
    public float[] Color { get; }
    public int GameOverReason { get; }
    public byte[] WinnablePlayers { get; }
    public int WinnableCampType { get; }

    public static SerializableWinnableData Of(WinnableData data)
    {
        var color = data.WinColor;
        return new SerializableWinnableData(data.Winnable, data.WinText,
            color == null ? Array.Empty<float>() : ColorToFloats((Color)color), (int)data.GameOverReason,
            data.WinnablePlayers.Select(p => p.PlayerId).ToArray(), (int)data.WinnableCampType);
    }

    public WinnableData ToWinnableData()
    {
        var toReturn = new WinnableData
        {
            Winnable = Winnable,
            WinText = WinText,
            WinColor = FloatsToColor(Color),
            GameOverReason = (GameOverReason) GameOverReason,
            WinnableCampType = (CampType) WinnableCampType
        };
        toReturn.WinnablePlayers.Clear();
        toReturn.WinnablePlayers.AddRange(WinnablePlayers.Select(p => PlayerUtils.GetPlayerById(p)!.Data));
        return toReturn;
    }

    private static float[] ColorToFloats(Color color)
    {
        var array = new float[4];
        array[0] = color.r;
        array[1] = color.g;
        array[2] = color.b;
        array[3] = color.a;

        return array;
    }
    
    private static Color FloatsToColor(float[] floats)
    {
        return new Color(floats[0], floats[1], floats[2], floats[3]);
    }
}