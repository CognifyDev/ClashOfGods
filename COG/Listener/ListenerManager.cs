using System.Collections.Generic;

namespace COG.Listener;

public class ListenerManager
{
    private static ListenerManager? _manager;

    private readonly List<IListener> _listeners = new();

    public static ListenerManager GetManager()
    {
        return _manager ??= new ListenerManager();
    }

    public void RegisterListener(IListener listener)
    {
        _listeners.Add(listener);
    }

    public void RegisterListeners(IListener[] listeners)
    {
        _listeners.AddRange(listeners);
    }

    public void UnregisterListener(IListener listener)
    {
        _listeners.Remove(listener);
    }

    public List<IListener> GetListeners()
    {
        return _listeners;
    }
}