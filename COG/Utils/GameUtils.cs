using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using COG.Role;
using COG.Role.Impl.Neutral;
using COG.Rpc;
using COG.States;
using UnityEngine;

namespace COG.Utils;

public static class GameUtils
{
    public static List<PlayerRole> PlayerRoleData { get; } = new();

    public static GenericPopup? Popup { get; set; }

    public const MurderResultFlags DefaultFlag = MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost;

    /// <summary>
    ///     向游戏里面发送一条信息
    /// </summary>
    /// <param name="text">信息内容</param>
    public static void SendGameMessage(string text)
    {
        if (DestroyableSingleton<HudManager>.Instance is { } hud)
            hud.Notifier.AddItem(text);
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
        return PlayerControl.LocalPlayer.GetMainRole();
    }

    /// <summary>
    ///     强制清除游戏状态
    /// </summary>
    public static void ForceClearGameData()
    {
        GameStates.InGame = false;
        PlayerRoleData.Clear();
        var gameManager = GameManager.Instance;
        if (gameManager != null) gameManager.EndGame();
    }

    public static NormalGameOptionsV07 GetGameOptions()
    {
        return GameOptionsManager.Instance.currentNormalGameOptions;
    }

    public static List<Transform> GetAllChildren(this Transform transform)
    {
        List<Transform> result = new();
        for (int i = 0; i < transform.childCount; i++)
            result.Add(transform.GetChild(i));
        return result;
    }
}