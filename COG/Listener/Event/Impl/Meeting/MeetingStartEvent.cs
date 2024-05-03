namespace COG.Listener.Event.Impl.Meeting;

public class MeetingStartEvent : MeetingEvent
{
    public MeetingStartEvent(MeetingHud meetingHud) : base(meetingHud)
    {
    }

    // 此方法只应进行Postfix
}