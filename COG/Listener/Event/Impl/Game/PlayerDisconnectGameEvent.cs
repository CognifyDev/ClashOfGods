using COG.Game.Events;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game;

public class PlayerDisconnectGameEvent : GameEventBase
{
    public PlayerDisconnectGameEvent(CustomPlayerData player) : base(GameEventType.Disconnect, player)
    {
    }
}