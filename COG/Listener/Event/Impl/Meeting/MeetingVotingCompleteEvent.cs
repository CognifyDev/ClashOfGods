namespace COG.Listener.Event.Impl.Meeting;

/// <summary>
///     此事件在投票结束后、结果公布时触发
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