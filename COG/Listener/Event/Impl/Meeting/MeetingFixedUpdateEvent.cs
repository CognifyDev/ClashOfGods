namespace COG.Listener.Event.Impl.Meeting;

public class MeetingFixedUpdateEvent : MeetingEvent
{
    public MeetingFixedUpdateEvent(MeetingHud meetingHud) : base(meetingHud)
    {
    }

    // 此方法只应进行Postfix
}