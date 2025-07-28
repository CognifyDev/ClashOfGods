using COG.Utils;
using System;

namespace COG.Game.Events;

public interface IGameEvent
{
    DateTime Time { get; }
    EventType EventType { get; }
    CustomPlayerData Player { get; }
}
