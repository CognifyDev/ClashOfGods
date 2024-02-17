using System.Collections.Generic;
using System.Linq;
using COG.NewListener.Event;

namespace COG.NewListener;

public class ListenerManager
{
    /// <summary>
    /// The instance of listener manager
    /// </summary>
    private static ListenerManager? _manager;

    /// <summary>
    /// Get the instance of listener manager
    /// </summary>
    /// <returns>instance</returns>
    public ListenerManager GetManager() => _manager ??= new ListenerManager();

    /// <summary>
    /// The list of listeners
    /// </summary>
    private readonly List<Handler> _handlers = new();

    /// <summary>
    /// Register a listener
    /// </summary>
    /// <param name="listener">the listener</param>
    public void RegisterListener(IListener listener)
    {
        foreach (var methodInfo in listener.GetType().GetMethods())
        {
            var attributes = methodInfo.GetCustomAttributes(typeof(EventHandlerAttribute), false);
            if (attributes.Length < 1) continue;
            _handlers.Add(new Handler(listener, methodInfo));
        }
    }

    /// <summary>
    /// Unregister handlers
    /// </summary>
    /// <param name="handlers"></param>
    public void UnRegisterHandlers(Handler[] handlers) => _handlers.RemoveAll(handlers.Contains);

    /// <summary>
    /// Register listeners
    /// </summary>
    /// <param name="listeners">listeners</param>
    public void RegisterListeners(IListener[] listeners) => listeners.ToList().ForEach(RegisterListener);

    /// <summary>
    /// Get the list of handlers
    /// </summary>
    /// <returns>handler list</returns>
    public Handler[] GetHandlers(IListener? listener = null)
        => listener == null ? _handlers.ToArray() : _handlers.Where(handler => listener.Equals(handler.Listener)).ToArray();

    /// <summary>
    /// Execute handlers
    /// </summary>
    public bool ExecuteHandlers(Event.Event @event)
    {
        // TODO 监听器执行
        
        return true;
    }
}