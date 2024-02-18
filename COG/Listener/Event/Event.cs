namespace COG.Listener.Event;

/// <summary>
/// 事件类
/// </summary>
public class Event
{
    public string Name { get; }
    
    protected Event()
    {
        Name = GetType().Name;
    }
}