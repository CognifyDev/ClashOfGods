using COG.Utils;

namespace COG.Game.Events.Impl;

public class SheriffMisfireGameEvent : GameEventBase
{
    public CustomPlayerData AttemptedTarget { get; }
    public SheriffMisfireGameEvent(CustomPlayerData sheriff, CustomPlayerData target) : base(GameEventType.Die, sheriff)
    {
        AttemptedTarget = target;
    }
}
