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

    // 此事件在会议hud销毁前，向玩家展示投票票数结果这个行为完成后发生
}

