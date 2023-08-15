using System.Collections.Generic;
using System.Linq;

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

    public void RegisterListeners(IEnumerable<IListener> listeners)
    {
        _listeners.AddRange(listeners);
    }

    public void UnregisterListener(IListener listener)
    {
        _listeners.Remove(listener);
    }

    public IListener? GetTypeListener<T>() where T : IListener
    {
        foreach (var listener in _listeners.OfType<T>()) return listener;

        return null;
    }

    public List<IListener> GetListeners()
    {
        return _listeners;
    }
}