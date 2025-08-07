using COG.Utils;

namespace COG.Game.Events.Impl;

public class StartMeetingEvent : GameEventBase
{
    public StartMeetingEvent(CustomPlayerData reporter, CustomPlayerData? dead) : base(GameEventType.StartMeeting,
        reporter)
    {
        DeadPlayer = dead;
    }

    public bool IsBodyReport => DeadPlayer != null;
    public CustomPlayerData? DeadPlayer { get; }
}