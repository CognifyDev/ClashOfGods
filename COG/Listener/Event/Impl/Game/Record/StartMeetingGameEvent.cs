using COG.Game.Events;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game.Record;

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