using COG.Utils;

namespace COG.Game.Events.Impl;

public class PlayerKillGameEvent : GameEventBase
{
    public CustomPlayerData Victim { get; }
    public IGameEvent RelatedDeathEvent { get; set; } = null!;

    public PlayerKillGameEvent(CustomPlayerData killer, CustomPlayerData victim) : base(GameEventType.Kill, killer)
    {
        Victim = victim;
    }
}
