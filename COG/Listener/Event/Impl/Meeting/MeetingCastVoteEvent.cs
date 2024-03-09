namespace COG.Listener.Event.Impl;

public class MeetingCastVoteEvent : MeetingEvent
{
    public PlayerControl Voter { get; }
    public PlayerControl Target { get; }
    public bool DidSkip { get; }

    public MeetingCastVoteEvent(MeetingHud meetingHud, PlayerControl voter, PlayerControl target, bool didSkip) : base(meetingHud)
    {
        Voter = voter;
        Target = target;
        DidSkip = didSkip;
    }
}
