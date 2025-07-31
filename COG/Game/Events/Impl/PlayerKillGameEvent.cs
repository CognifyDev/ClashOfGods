using COG.Utils;

namespace COG.Game.Events.Impl;

public class PlayerKillGameEvent : GameEventBase
{
    public CustomPlayerData Victim { get; }

    public PlayerKillGameEvent(CustomPlayerData killer, CustomPlayerData victim) : base(GameEventType.Kill, killer)
    {
        Victim = victim;
    }
}
