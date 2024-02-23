using System;
using System.Linq;
using COG.Listener;
using COG.Listener.Event;
using COG.Utils.Coding;
using NLua;

namespace COG.Plugin.Loader.Controller.Listener;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
public class ListenerController
{
    public Lua Lua { get; }
    public IPlugin Plugin { get; }
    
    public ListenerController(Lua lua, IPlugin plugin)
    {
        Lua = lua;
        Plugin = plugin;
    }

    /// <summary>
    /// 注册一个监听器
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="methodName">函数名称</param>
    public void RegisterListener(string eventName, string methodName)
    {
        var function = Lua.GetFunction(methodName);
        var controllerType = typeof(ListenerControllerListener<>);
        var eventType = Event.GetSubClasses().FirstOrDefault(subclass => eventName.Equals(subclass.Name));

        if (eventType == null) return;
        var constructedType = controllerType.MakeGenericType(eventType);
        
        var instance = Activator.CreateInstance(constructedType, function) as IListener;
        ListenerManager.GetManager().RegisterListener(instance!);
    }

    [DontDelete]
    private class ListenerControllerListener<T> : IListener where T : Event
    {
        private readonly LuaFunction _function;
        
        public ListenerControllerListener(LuaFunction function)
        {
            _function = function;
        }
        
        [EventHandler(EventHandlerType.Prefix)]
        private bool OnPrefixEventHandle(T @event)
        {
            return (bool) _function.Call(@event)[0];
        }

        [EventHandler(EventHandlerType.Postfix)]
        public void OnPostfixEventHandle(T @event)
        {
            _function.Call(@event);
        }
    }
}