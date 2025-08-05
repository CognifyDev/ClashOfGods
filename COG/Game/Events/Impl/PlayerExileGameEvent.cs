using COG.Utils;

namespace COG.Game.Events.Impl;

public class PlayerExileGameEvent : GameEventBase
{
    public PlayerExileGameEvent(CustomPlayerData exiled) : base(GameEventType.Exile, exiled)
    {
    }
}
