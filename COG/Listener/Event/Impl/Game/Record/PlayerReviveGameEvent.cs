using COG.Game.Events;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game.Record;

public class PlayerReviveGameEvent : GameEventBase
{
    public PlayerReviveGameEvent(CustomPlayerData revived, CustomPlayerData? reviver = null) : base(
        GameEventType.Revive, revived)
    {
        Reviver = reviver;
    }

    public CustomPlayerData? Reviver { get; }
}