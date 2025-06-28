using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using COG.Listener.Event;
using COG.Role;
using COG.States;
using COG.Utils;

namespace COG.Listener;

public class ListenerManager
{
    /// <summary>
    ///     The instance of listener manager
    /// </summary>
    private static ListenerManager? _manager;

    /// <summary>
    ///     The list of listeners
    /// </summary>
    private readonly List<Handler> _handlers = new();

    /// <summary>
    ///     Get the instance of a listener manager
    /// </summary>
    /// <returns>instance</returns>
    public static ListenerManager GetManager()
    {
        return _manager ??= new ListenerManager();
    }

    /// <summary>
    ///     Register a listener
    /// </summary>
    /// <param name="listener">the listener</param>
    public void RegisterListener(IListener listener)
    {
        RegisterHandlers(AsHandlers(listener));
    }

    public void RegisterHandlers(Handler[] handlers)
    {
        handlers.ForEach(handler =>
        {
            _handlers.Add(handler);
            Main.Logger.LogDebug(
                $"Registered listener handler => {handler.Method.Name} from {handler.Listener.GetType().Name} by type of {handler.EventHandlerType}");
        });
    }

    public Handler[] AsHandlers(IListener listener)
    {
        return (from methodInfo in listener.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly)
            let attributes = methodInfo.GetCustomAttributes(typeof(EventHandlerAttribute), false) 
            where attributes.Length >= 1 let attribute = attributes[0] as EventHandlerAttribute 
            let type = attribute!.EventHandlerType select new Handler(listener, methodInfo, type)).ToArray();
    }

    /// <summary>
    ///     Register a listener if it does not exist
    /// </summary>
    /// <param name="listener">the listener</param>
    public void RegisterListenerIfNotExists(IListener listener)
    {
        if (GetHandlers(listener).ToList().IsEmpty()) 
            RegisterListener(listener);
    }

    /// <summary>
    ///     Unregister handlers
    /// </summary>
    /// <param name="handlers"></param>
    public void UnRegisterHandlers(Handler[] handlers)
    {
        _handlers.RemoveAll(handlers.Contains);
    }

    /// <summary>
    ///     Unregister all handlers
    /// </summary>
    public void UnregisterHandlers()
    {
        _handlers.Clear();
    }

    /// <summary>
    ///     Register listeners
    /// </summary>
    /// <param name="listeners">listeners</param>
    public void RegisterListeners(IListener[] listeners)
    {
        listeners.ToList().ForEach(RegisterListener);
    }

    /// <summary>
    ///     Get the list of handlers
    /// </summary>
    /// <returns>handler list</returns>
    public IEnumerable<Handler> GetHandlers(IListener? listener = null)
    {
        return listener == null
            ? _handlers.ToArray()
            : _handlers.Where(handler => listener.Equals(handler.Listener)).ToArray();
    }

    /// <summary>
    ///     Execute handlers
    /// </summary>
    public bool ExecuteHandlers(Event.Event @event, EventHandlerType type)
    {
        var toReturn = true;
        
        /* for only
         if you try to use foreach, then
         [Error  :Il2CppInterop] During invoking native->managed trampoline
         Exception: System.InvalidOperationException: Collection was modified; enumeration operation may not execute.
         at System.Collections.Generic.List`1.Enumerator.MoveNextRare()
         at System.Collections.Generic.List`1.Enumerator.MoveNext()
         at COG.Listener.ListenerManager.ExecuteHandlers(Event event, EventHandlerType type) in D:\RiderProjects\ClashOfGods\COG\Listener\ListenerManager.cs:line 123
         at COG.Patch.GameStartPatch.Postfix(GameManager __instance) in D:\RiderProjects\ClashOfGods\COG\Patch\GamePatch.cs:line 267
         at DMD<GameManager::StartGame>(GameManager this)
         at (il2cpp -> managed) StartGame(IntPtr , Il2CppMethodInfo* )
         */
        for (var i = 0; i < _handlers.Count; i++)
        {
            var handler = _handlers[i];
            if (!type.Equals(handler.EventHandlerType) ||
                !handler.EventType.IsInstanceOfType(@event)) continue;

            var onlyLocal = handler.Method.GetCustomAttribute<OnlyLocalPlayerInvokableAttribute>();
            if (onlyLocal != null && handler.Listener is CustomRole role)
                if (!role.IsLocalPlayerRole())
                    continue;

            var onlyInRealGame = handler.Method.GetCustomAttribute<OnlyInRealGameAttribute>();
            if (onlyInRealGame != null && !GameStates.InRealGame)
                continue;

            var returnType = handler.Method.ReturnType;
            var result = handler.Method.Invoke(handler.Listener, new object?[] { @event });

            if (type == EventHandlerType.Prefix && returnType == typeof(bool) && !(bool)result!) toReturn = false;
        }

        return toReturn;
    }
}
