using COG.Utils;

namespace COG.Game.Events.Impl;

public class PlayerDieGameEvent : GameEventBase
{
    public PlayerDieGameEvent(CustomPlayerData dead) : base(GameEventType.Die, dead)
    {
    }
}