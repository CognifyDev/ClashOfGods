using JetBrains.Annotations;

namespace COG.Listener.Event.Impl;
public class MeetingVotingCompleteEvent : MeetingEvent
{
    public MeetingHud.VoterState[] States { get; }
    public GameData.PlayerInfo Exiled { get; }
    public bool Tie { get; }
    public MeetingVotingCompleteEvent(MeetingHud meetingHud, MeetingHud.VoterState[] states, GameData.PlayerInfo exiled, bool tie) : base(meetingHud)
    {
        States = states;
        Exiled = exiled;
        Tie = tie;
    }
}

