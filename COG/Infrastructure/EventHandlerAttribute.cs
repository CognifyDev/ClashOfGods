using System;

namespace COG.Infrastructure;

[AttributeUsage(AttributeTargets.Method)]
public class EventHandlerAttribute : Attribute
{
    public GameEventType EventType { get; }
    
    public EventHandlerAttribute(GameEventType eventType)
    {
        EventType = eventType;
    }
}
