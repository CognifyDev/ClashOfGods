using COG.Utils;

namespace COG.Game.Events.Impl;

public class WitchRevivedInteractionDieGameEvent : GameEventBase
{
    public WitchRevivedInteractionDieGameEvent(CustomPlayerData witch, CustomPlayerData revived) : base(
        GameEventType.Die, witch)
    {
        Revived = revived;
    }

    public CustomPlayerData Revived { get; }
}