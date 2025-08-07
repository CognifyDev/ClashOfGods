using COG.Game.Events;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game;

public class PlayerExileGameEvent : GameEventBase
{
    public PlayerExileGameEvent(CustomPlayerData exiled) : base(GameEventType.Exile, exiled)
    {
    }
}