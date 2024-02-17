namespace COG.NewListener.Event;

/// <summary>
/// 事件类
/// </summary>
public class Event
{
    public string Name { get; }

    public EventHandlerType EventHandlerType;

    public bool Cancel { get; protected set; }
    
    protected Event(EventHandlerType type)
    {
        Name = GetType().Name;
        EventHandlerType = type;
    }

    public void SetCancellable() => Cancel = true;
}