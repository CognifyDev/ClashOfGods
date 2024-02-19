using System;
using System.Collections.Generic;
using System.Linq;
using COG.Listener.Event;

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
        foreach (var handler in from handler in _handlers where handler != null! where type.Equals(handler.EventHandlerType) where handler.EventType.IsInstanceOfType(@event) select handler)
        {
            switch (type)
            {
                case EventHandlerType.Prefix:
                    if (handler.Method.ReturnType.IsAssignableFrom(typeof(bool)))
                        return (bool)handler.Method.Invoke(handler.Listener, new object?[] { @event })!;
                    handler.Method.Invoke(handler.Listener, new object?[] { @event });
                    return true;

                case EventHandlerType.Postfix:
                    handler.Method.Invoke(handler.Listener, new object?[] { @event });
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        return true;
        /*
        foreach (var handler in _handlers.Where(handler => handler.EventType.IsInstanceOfType(@event)))
        {
            // 当返回值为bool的时候，代表想要获取Listener的相关信息并且操作，做操作，所以直接执行
            if (handler.Method.ReturnType.IsAssignableFrom(typeof(bool)))
            {
                var result = (bool) handler.Method.Invoke(handler.Listener, new object?[] { @event })!;
                return result ? 1 : 0; // 真 是1，假 是0
            }

            handler.Method.Invoke(handler.Listener, new object?[] { @event });
        }

        // 无操作是-1
        return -1;
        */
    }
}