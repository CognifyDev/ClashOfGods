using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using COG.Listener.Impl;
using COG.Rpc;
using COG.States;

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
        var player = PlayerControl.LocalPlayer;
        return (from playerRole in PlayerRoleData where playerRole.Player.Equals(player) select playerRole.Role)
            .FirstOrDefault();
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

        GameListener.ClearRoleListeners();
    }

    public static NormalGameOptionsV07 GetGameOptions()
    {
        return GameOptionsManager.Instance.currentNormalGameOptions;
    }

    public static void SetCustomRole(this PlayerControl pc, Role.Role role)
    {
        if (!pc) return;
        var playerRole = PlayerRoleData.FirstOrDefault(pr => pr.Player.IsSamePlayer(pc));
        if (playerRole is not null) PlayerRoleData.Remove(playerRole);
        PlayerRoleData.Add(new(pc, role));
        pc.SetRole(role.BaseRoleType);
        Main.Logger.LogInfo($"The role of player {pc.Data.PlayerName} was set to {role.GetType().Name}");
    }

    public static void RpcSetCustomRole(this PlayerControl pc, Role.Role role)
    {
        if (!pc) return;
        var writer = RpcUtils.StartRpcImmediately(pc, KnownRpc.SetRole);
        writer.Write(pc.PlayerId);
        writer.WritePacked(role.Id);
        writer.Finish();
        SetCustomRole(pc, role);
    }
}