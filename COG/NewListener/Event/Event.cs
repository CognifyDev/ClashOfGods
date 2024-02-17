namespace COG.NewListener.Event;

/// <summary>
/// 事件类
/// </summary>
public class Event
{
    public string Name { get; }

    public bool Cancel { get; protected set; }
    
    protected Event()
    {
        Name = GetType().Name;
    }

    public void SetCancellable() => Cancel = true;
}