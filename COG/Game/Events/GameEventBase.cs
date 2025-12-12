using COG.Rpc;
using COG.Utils;
using System;
using UnityEngine.Analytics;

namespace COG.Game.Events;

public abstract class GameEventBase(GameEventType eventType, CustomPlayerData? player) : IGameEvent
{
    public DateTime Time { get; } = DateTime.Now;
    public GameEventType EventType { get; } = eventType;
    public CustomPlayerData? Player { get; } = player;
}

public abstract class NetworkedGameEventBase<TEvent, TSender>(GameEventType eventType, CustomPlayerData? player)
    : GameEventBase(eventType, player)
    where TEvent : NetworkedGameEventBase<TEvent, TSender>
    where TSender : NetworkedGameEventSender<TSender, TEvent>, new()
{
    public TSender EventSender => NetworkedGameEventSender<TSender, TEvent>.Instance;
}

public abstract class NetworkedGameEventSender<TSelf, TEvent>
    where TSelf : NetworkedGameEventSender<TSelf, TEvent>, new()
    where TEvent : NetworkedGameEventBase<TEvent, TSelf>
{
    private static TSelf? _instance;
    public static TSelf Instance => _instance ??= new TSelf();

    public abstract void Serialize(RpcWriter writer, TEvent correspondingEvent);

    public abstract TEvent Deserialize(MessageReader reader);
}