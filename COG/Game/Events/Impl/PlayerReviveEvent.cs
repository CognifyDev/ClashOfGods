using COG.Utils;

namespace COG.Game.Events.Impl;

public class PlayerReviveEvent : GameEventBase
{
    public CustomPlayerData? Reviver { get; }

    public PlayerReviveEvent(CustomPlayerData revived, CustomPlayerData? reviver = null) : base(EventType.Revive, revived)
    {
        Reviver = reviver;
    }
}
