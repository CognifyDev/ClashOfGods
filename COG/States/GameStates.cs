global using GameStates = COG.States.GameStates;
using COG.Patch;
using COG.Role;
using COG.UI.Vanilla.KillButton;
using COG.Utils;
using System;

namespace COG.States;

/// <summary>
///     与游戏相关状态
/// </summary>
public static class GameStates
{
    private static bool _inRealGame = false;

    /// <summary>
    ///     是否处于正式游戏中
    /// </summary>
    public static bool InRealGame
    {
        get => _inRealGame;

        set
        {
            Main.Logger.LogMessage($"{nameof(GameStates)}::{nameof(_inRealGame)} being set to {value}. Clearing role data...");

            CustomRoleManager.GetManager().GetRoles().ForEach(r => r.ClearRoleGameData());

            VanillaKillButtonPatch.ActiveLastFrame = false;
            
            _inRealGame = value;

            if (!value)
            {
                Main.Logger.LogMessage("Player left game. Clearing in-game data...");

                GameUtils.PlayerData.Clear();
                DeadPlayer.DeadPlayers.Clear();
            }
        }
    }

    /// <summary>
    ///     是否在大厅中
    /// </summary>
    public static bool InLobby => LobbyBehaviour.Instance;

    /// <summary>
    ///     是否处于会议中
    /// </summary>
    public static bool IsMeeting => InRealGame && MeetingHud.Instance;

    public static bool IsOnlineGame => AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;

    /// <summary>
    ///     是否处于投票阶段
    /// </summary>
    public static bool IsVoting => IsMeeting &&
                                   MeetingHud.Instance.state is MeetingHud.VoteStates.Voted
                                       or MeetingHud.VoteStates.NotVoted;
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class OnlyInRealGameAttribute : System.Attribute
{
}