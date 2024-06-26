using COG.Role;
using InnerNet;

namespace COG.States;

/// <summary>
///     与游戏相关状态
/// </summary>
public static class GameStates
{
    private static bool _inGame;

    /// <summary>
    ///     是否处于游戏中
    /// </summary>
    public static bool InGame
    {
        get => _inGame;

        set
        {
            CustomRoleManager.GetManager().GetRoles().ForEach(r => r.ClearRoleGameData());
            
            _inGame = value;
        }
    }

    /// <summary>
    ///     是否在大厅中
    /// </summary>
    public static bool IsLobby => AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined;

    /// <summary>
    ///     是否处于会议中
    /// </summary>
    public static bool IsMeeting => InGame && MeetingHud.Instance;

    public static bool IsOnlineGame => AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame;

    /// <summary>
    ///     是否处于投票阶段
    /// </summary>
    public static bool IsVoting => IsMeeting &&
                                   MeetingHud.Instance.state is MeetingHud.VoteStates.Voted
                                       or MeetingHud.VoteStates.NotVoted;
}