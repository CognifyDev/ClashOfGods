using COG.Game.Events;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game;

public class WitchRevivedInteractionDieGameEvent : GameEventBase
{
    public WitchRevivedInteractionDieGameEvent(CustomPlayerData witch, CustomPlayerData revived) : base(
        GameEventType.Die, witch)
    {
        Revived = revived;
    }

    public CustomPlayerData Revived { get; }
}