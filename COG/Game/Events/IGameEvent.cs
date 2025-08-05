using COG.Utils;
using System;

namespace COG.Game.Events;

public interface IGameEvent
{
    DateTime Time { get; }
    GameEventType EventType { get; }
    CustomPlayerData Player { get; }
}
