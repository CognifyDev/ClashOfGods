using System;
using COG.Utils;

namespace COG.Game.Events;

public abstract class GameEventBase : IGameEvent
{
    public GameEventBase(GameEventType eventType, CustomPlayerData? player)
    {
        Time = DateTime.Now;
        EventType = eventType;
        Player = player;
    }

    public DateTime Time { get; }
    public GameEventType EventType { get; }
    public CustomPlayerData? Player { get; }
}