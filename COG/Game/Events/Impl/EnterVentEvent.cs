using COG.Utils;

namespace COG.Game.Events.Impl;

public class EnterVentEvent : GameEventBase
{
    public int VentId { get; }

    public EnterVentEvent(CustomPlayerData player, int ventId) : base(EventType.EnterVent, player)
    {
        VentId = ventId;
    }
}
