using COG.Utils;

namespace COG.Game.Events.Impl;

public class PlayerReviveGameEvent : GameEventBase
{
    public CustomPlayerData? Reviver { get; }

    public PlayerReviveGameEvent(CustomPlayerData revived, CustomPlayerData? reviver = null) : base(GameEventType.Revive, revived)
    {
        Reviver = reviver;
    }
}
