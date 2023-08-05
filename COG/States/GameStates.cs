using InnerNet;

namespace COG.States;

public class GameStates
{
    public static bool InGame = false;
    public static bool IsLobby => AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined;
    public static bool IsInTask => InGame && !MeetingHud.Instance;
    public static bool IsMeeting => InGame && MeetingHud.Instance;

    public static bool IsVoting => IsMeeting &&
                                   MeetingHud.Instance.state is MeetingHud.VoteStates.Voted
                                       or MeetingHud.VoteStates.NotVoted;
}