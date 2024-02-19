using System;
using System.Collections.Generic;
using System.Linq;
using COG.Listener.Event;
using COG.Listener.Event.Impl.ICutscene;

namespace COG.Listener;

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
    public static ListenerManager GetManager() => _manager ??= new ListenerManager();

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
            var attribute = attributes[0] as EventHandlerAttribute;
            var type = attribute!.EventHandlerType;
            _handlers.Add(new Handler(listener, methodInfo, type));
            Main.Logger.LogInfo($"Registered listener handler => {methodInfo.Name} from {listener.GetType().Name} by type of {type.ToString()}");
        }
    }

    /// <summary>
    /// Unregister handlers
    /// </summary>
    /// <param name="handlers"></param>
    public void UnRegisterHandlers(Handler[] handlers) => _handlers.RemoveAll(handlers.Contains);

    /// <summary>
    /// Unregister all handlers
    /// </summary>
    public void UnRegisterHandlers() => _handlers.Clear();

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
    public bool ExecuteHandlers(Event.Event @event, EventHandlerType type)
    {
        var toReturn = true;

        foreach (var handler in _handlers)
        {
            if (!type.Equals(handler.EventHandlerType) ||
                !handler.EventType.IsInstanceOfType(@event)) continue;

            var returnType = handler.Method.ReturnType;
            var result = handler.Method.Invoke(handler.Listener, new object?[] { @event });

            if (type == EventHandlerType.Prefix && returnType == typeof(bool) && !(bool)result!)
            {
                toReturn = false;
            }
        }

        return toReturn;
    }
}