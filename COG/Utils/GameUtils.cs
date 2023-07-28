using System;
using System.Collections.Generic;
using AmongUs.GameOptions;

namespace COG.Utils;

public class GameUtils
{
    public static readonly Dictionary<PlayerControl, Role.Role> Data = new();

    /// <summary>
    /// 向游戏里面发送一条信息
    /// </summary>
    /// <param name="text">信息内容</param>
    public static void SendGameMessage(string text)
    {
        if (DestroyableSingleton<HudManager>._instance)
            DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(text);
    }

    public static int GetImpostorsNum()
    {
        var players = PlayerUtils.GetAllPlayers();
        if (players == null)
        {
            throw new NullReferenceException("Player list is null");
        }
    
        int playersNum = players.Count;
        int impostorsNum;
    
        switch (playersNum)
        {
            case <= 6:
                impostorsNum = 1;
                break;
            case <= 8:
                impostorsNum = 2;
                break;
            default:
                impostorsNum = 3;
                break;
        }
    
        return impostorsNum;
    }

    public static NormalGameOptionsV07 GetGameOptions() => GameOptionsManager.Instance.currentNormalGameOptions;
}