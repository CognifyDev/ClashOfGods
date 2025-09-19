using COG.Game.Events;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game.Record;

public class PlayerDieGameEvent : GameEventBase
{
    public PlayerDieGameEvent(CustomPlayerData dead) : base(GameEventType.Die, dead)
    {
    }
}