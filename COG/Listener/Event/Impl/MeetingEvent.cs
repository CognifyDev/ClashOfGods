namespace COG.Listener.Event.Impl;

/// <summary>
///     这个事件为关于会议界面操作的总事件
///     所有关于会议界面的事件类都是此事件的子类
/// </summary>
public class MeetingEvent : Event
{
    public MeetingEvent(MeetingHud meeting)
    {
        MeetingHud = meeting;
    }

    /// <summary>
    ///     本地会议界面实例
    /// </summary>
    public MeetingHud MeetingHud { get; }
}