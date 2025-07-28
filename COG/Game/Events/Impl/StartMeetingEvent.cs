using COG.Utils;

namespace COG.Game.Events.Impl;

public class StartMeetingEvent : GameEventBase
{
    public bool IsBodyReport => DeadPlayer != null;
    public CustomPlayerData? DeadPlayer { get; }

    public StartMeetingEvent(CustomPlayerData reporter, CustomPlayerData? dead) : base(EventType.StartMeeting, reporter)
    {
        DeadPlayer = dead;
    }
}
