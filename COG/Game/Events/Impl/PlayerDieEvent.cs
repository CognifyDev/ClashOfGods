using COG.Utils;

namespace COG.Game.Events.Impl;

public class PlayerDieEvent : GameEventBase
{
    public PlayerDieEvent(CustomPlayerData dead) : base(EventType.Die, dead)
    {
    }
}
