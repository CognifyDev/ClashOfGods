using COG.Game.Events;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game;

public class PlayerDieGameEvent : GameEventBase
{
    public PlayerDieGameEvent(CustomPlayerData dead) : base(GameEventType.Die, dead)
    {
    }
}