using COG.Rpc;
using COG.Utils;
using COG.Utils.Coding;

namespace COG.Game.Events;

/// <summary>
///     NOTE: The subclasses must have two constructors, one for sending the event and one for deserializing the event
/// </summary>
[WorkInProgress]
public abstract class NetworkedGameEventBase : GameEventBase
{
    protected NetworkedGameEventBase(GameEventType eventType, CustomPlayerData player) : base(eventType, player)
    {
    }

    public abstract void Serialize(RpcWriter writer);

    /// <summary>
    ///     Called right after constructing.
    /// </summary>
    /// <param name="reader"></param>
    public abstract void Deserialize(MessageReader reader);

    protected void DoSend() // Sub-class constructors are called after the base constructor, so we should manually call it instead of calling Serialize in the constructor.
    {
        var writer = RpcWriter.Start(KnownRpc.SyncGameEvent);
        writer.Write(GetType().Name);
        writer.WriteBytesAndSize(SerializablePlayerData.Of(Player!).SerializeToData());
        Serialize(writer);
        writer.Finish();
    }
}