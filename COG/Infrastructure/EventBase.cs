using System;

namespace COG.Infrastructure;

public abstract class EventBase : IEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
