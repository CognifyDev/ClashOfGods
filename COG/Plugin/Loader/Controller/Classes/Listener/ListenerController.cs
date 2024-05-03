using System;
using System.Linq;
using COG.Listener;
using COG.Listener.Event;
using COG.Utils.Coding;
using NLua;

namespace COG.Plugin.Loader.Controller.Classes.Listener;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
public class ListenerController
{
    private Lua Lua { get; }
    private IPlugin Plugin { get; }

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
    /// <param name="prefix">是否是Prefix模式</param>
    public void RegisterListener(string eventName, string methodName, bool prefix)
    {
        var function = Lua.GetFunction(methodName);
        var controllerType = typeof(ListenerControllerListener<>);
        var eventType = Event.GetSubClasses().FirstOrDefault(subclass => eventName.Equals(subclass.Name));

        if (eventType == null) return;
        var constructedType = controllerType.MakeGenericType(eventType);

        var instance = Activator.CreateInstance(constructedType, function, prefix) as IListener;
        ListenerManager.GetManager().RegisterListener(instance!);
    }

    [DontDelete]
    private class ListenerControllerListener<T> : IListener where T : Event
    {
        private readonly LuaFunction _function;
        private readonly bool _prefix;

        public ListenerControllerListener(LuaFunction function, bool prefix)
        {
            _function = function;
            _prefix = prefix;
        }

        [EventHandler(EventHandlerType.Prefix)]
        private bool OnPrefixEventHandle(T @event)
        {
            if (!_prefix) return true;
            var result = _function.Call(@event);
            if (result is not { Length: > 0 }) return true;
            var toReturn = result[0];
            return toReturn switch
            {
                null => true,
                bool @return => @return,
                _ => true
            };
        }

        [EventHandler(EventHandlerType.Postfix)]
        private void OnPostfixEventHandle(T @event)
        {
            if (_prefix) return;
            _function.Call(@event);
        }
    }
}