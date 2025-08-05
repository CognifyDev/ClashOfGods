using COG.Utils;
using System;

namespace COG.Game.Events;

public abstract class GameEventBase : IGameEvent
{
    public DateTime Time { get; }
    public GameEventType EventType { get; }
    public CustomPlayerData? Player { get; }

    public GameEventBase(GameEventType eventType, CustomPlayerData? player)
    {
        Time = DateTime.Now;
        EventType = eventType;
        Player = player;
    }
}
