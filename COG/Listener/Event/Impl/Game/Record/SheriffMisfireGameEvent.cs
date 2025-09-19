using COG.Game.Events;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game.Record;

public class SheriffMisfireGameEvent : GameEventBase
{
    public SheriffMisfireGameEvent(CustomPlayerData sheriff, CustomPlayerData target) : base(GameEventType.Die, sheriff)
    {
        AttemptedTarget = target;
    }

    public CustomPlayerData AttemptedTarget { get; }
}