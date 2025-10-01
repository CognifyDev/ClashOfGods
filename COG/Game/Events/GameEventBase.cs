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

public interface INetworkedGameEventBase
{
}

public abstract class NetworkedGameEventBase<T> : GameEventBase, INetworkedGameEventBase where T : INetworkedGameEventSender
{
    public NetworkedGameEventBase(GameEventType eventType, CustomPlayerData? player) : base(eventType, player)
    {
    }

    public T EventSender => (T)INetworkedGameEventSender.AllSenders.First(s => s is T);
}

public interface INetworkedGameEventSender
{
    public static List<INetworkedGameEventSender> AllSenders { get; } = new();

    public string Id { get; }
}

public abstract class NetworkedGameEventSender<T> : INetworkedGameEventSender where T : INetworkedGameEventBase
{
    public NetworkedGameEventSender(string id)
    {
        Id = id;
    }

    public string Id { get; }

    public abstract void Serialize(RpcWriter writer, T correspondingEvent);

    public abstract T Deserialize(MessageReader reader);
}