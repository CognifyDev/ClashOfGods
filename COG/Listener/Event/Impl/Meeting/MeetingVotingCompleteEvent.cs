namespace COG.Listener.Event.Impl.Meeting;

/// <summary>
///     此事件在会议Hud销毁前，向玩家展示投票票数结果这个行为完成后发生
/// </summary>
public class MeetingVotingCompleteEvent : MeetingEvent
{
    public MeetingVotingCompleteEvent(MeetingHud meetingHud, MeetingHud.VoterState[] states, NetworkedPlayerInfo exiled,
        bool tie) : base(meetingHud)
    {
        States = states;
        Exiled = exiled;
        Tie = tie;
    }

    public MeetingHud.VoterState[] States { get; }
    public NetworkedPlayerInfo Exiled { get; }
    public bool Tie { get; }
}