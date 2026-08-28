using System;

namespace COG.Infrastructure;

public interface IEvent
{
    DateTime Timestamp { get; }
}
