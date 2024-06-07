using System;
using System.Reflection;

namespace COG.Listener.Event;

/// <summary>
///     监听器注册实例
/// </summary>
public class Handler
{
    public readonly EventHandlerType EventHandlerType;
    public readonly Type EventType;

    public readonly IListener Listener;

    public readonly MethodInfo Method;

    public Handler(IListener listener, MethodInfo method, EventHandlerType type)
    {
        Listener = listener;
        var parameterInfos = method.GetParameters();
        if (parameterInfos.Length != 1) throw new System.Exception("not a event method");
        var parameter = parameterInfos[0];
        EventType = parameter.ParameterType;
        if (!EventType.IsSubclassOf(typeof(Event))) throw new System.Exception("the input type is not a Event type");

        Method = method;
        EventHandlerType = type;
    }
}