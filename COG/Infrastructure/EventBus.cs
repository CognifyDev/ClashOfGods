using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace COG.Infrastructure;

public class EventBus
{
    private static EventBus? _instance;
    public static EventBus Instance => _instance ??= new EventBus();
    
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    private readonly object _lock = new();
    
    public void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        lock (_lock)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();
            _handlers[type].Add(handler);
        }
    }
    
    public void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        lock (_lock)
        {
            var type = typeof(T);
            if (_handlers.ContainsKey(type))
                _handlers[type].Remove(handler);
        }
    }
    
    public void Publish<T>(T eventData) where T : IEvent
    {
        List<Delegate> handlers;
        lock (_lock)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                return;
            handlers = _handlers[type].ToList();
        }
        
        foreach (var handler in handlers)
        {
            try
            {
                ((Action<T>)handler)(eventData);
            }
            catch (System.Exception ex)
            {
                Main.Logger.LogError($"Error in event handler: {ex}");
            }
        }
    }
    
    public void RegisterHandlers(object handler)
    {
        var methods = handler.GetType().GetMethods(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        
        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<EventHandlerAttribute>();
            if (attr == null) continue;
            
            var parameters = method.GetParameters();
            if (parameters.Length != 1) continue;
            
            var paramType = parameters[0].ParameterType;
            var delegateType = typeof(Action<>).MakeGenericType(paramType);
            var del = Delegate.CreateDelegate(delegateType, handler, method);
            
            var subscribeMethod = typeof(EventBus)
                .GetMethod(nameof(Subscribe))!
                .MakeGenericMethod(paramType);
            subscribeMethod.Invoke(this, new object[] { del });
        }
    }
    
    public void Clear()
    {
        lock (_lock)
        {
            _handlers.Clear();
        }
    }
}
