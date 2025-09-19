using COG.Game.Events;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game.Record;

public class PlayerKillGameEvent : GameEventBase
{
    public PlayerKillGameEvent(CustomPlayerData killer, CustomPlayerData victim) : base(GameEventType.Kill, killer)
    {
        Victim = victim;
    }

    public CustomPlayerData Victim { get; }
    public IGameEvent RelatedDeathEvent { get; set; } = null!;
}