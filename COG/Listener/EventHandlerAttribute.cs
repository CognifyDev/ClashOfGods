using System;

namespace COG.Listener;

/// <summary>
/// The attribute used to mark a method as a listener method
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class EventHandlerAttribute : Attribute
{
    public EventHandlerType EventHandlerType { get; }
    
    public EventHandlerAttribute(EventHandlerType type)
    {
        EventHandlerType = type;
    }
}