using COG.Game.Events;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game.Record;

public class PlayerGuessGameEvent: GameEventBase
{
    public PlayerGuessGameEvent(CustomPlayerData guesser, CustomPlayerData victim) : base(GameEventType.Guess, guesser)
    {
        Victim = victim;
    }

    public CustomPlayerData Victim { get; }
    public IGameEvent RelatedDeathEvent { get; set; } = null!;
}