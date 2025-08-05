using COG.Utils;

namespace COG.Game.Events.Impl;

public class EnterVentGameEvent : GameEventBase
{
    public int VentId { get; }

    public EnterVentGameEvent(CustomPlayerData player, int ventId) : base(GameEventType.EnterVent, player)
    {
        VentId = ventId;
    }
}
