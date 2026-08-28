using System;
using AmongUs.GameOptions;

namespace COG.Infrastructure;

public class GameEvent<TData> : EventBase where TData : class
{
    public GameEventType EventType { get; }
    public TData? Data { get; }
    public PlayerControl? Player { get; }

    public GameEvent(GameEventType eventType, TData? data = null, PlayerControl? player = null)
    {
        EventType = eventType;
        Data = data;
        Player = player;
    }
}

public class GameEvent : EventBase
{
    public GameEventType EventType { get; }
    public PlayerControl? Player { get; }

    public GameEvent(GameEventType eventType, PlayerControl? player = null)
    {
        EventType = eventType;
        Player = player;
    }
}
