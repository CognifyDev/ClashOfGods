using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using COG.Listener;
using COG.Listener.Impl;
using COG.States;

namespace COG.Utils;

public static class GameUtils
{
    public static Dictionary<PlayerControl, Role.Role> Data { get; internal set; } = new();

    /// <summary>
    ///     向游戏里面发送一条信息
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
        if (players == null) throw new NullReferenceException("Player list is null");

        var playersNum = players.Count;
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

    public static Role.Role? GetLocalPlayerRole()
    {
        var player = PlayerControl.LocalPlayer;
        return Data.Where(keyValuePair => keyValuePair.Key.IsSamePlayer(player))
            .Select(keyValuePair => keyValuePair.Value).FirstOrDefault();
    }

    /// <summary>
    ///     强制清除游戏状态
    /// </summary>
    public static void ForceClearGameData()
    {
        GameStates.InGame = false;
        Data.Clear();
        var gameManager = GameManager.Instance;
        if (gameManager != null) gameManager.EndGame();

        var gameListener = ListenerManager.GetManager().GetTypeListener<GameListener>()!;

        try
        {
            gameListener.OnGameEndSetEverythingUp(null!);
        }
        catch
        {
            // ignored
        }
    }

    public static NormalGameOptionsV07 GetGameOptions()
    {
        return GameOptionsManager.Instance.currentNormalGameOptions;
    }
}