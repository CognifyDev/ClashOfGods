using System;
using System.Collections.Generic;
using System.Linq;
using COG.Rpc;
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

public abstract class NetworkedGameEventBase<TEvent, TSender> : GameEventBase 
    where TEvent : NetworkedGameEventBase<TEvent, TSender>
    where TSender : NetworkedGameEventSender<TSender, TEvent>, new()
{
    public NetworkedGameEventBase(GameEventType eventType, CustomPlayerData? player) : base(eventType, player)
    {
    }

    public TSender EventSender => NetworkedGameEventSender<TSender, TEvent>.Instance;
}

public abstract class NetworkedGameEventSender<TSelf, TEvent> where TSelf : NetworkedGameEventSender<TSelf, TEvent>, new()
{
    public static TSelf Instance => _instance ??= new();
    private static TSelf? _instance;

    public abstract void Serialize(RpcWriter writer, TEvent correspondingEvent);

    public abstract TEvent Deserialize(MessageReader reader);
}