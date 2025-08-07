using COG.Utils;

namespace COG.Game.Events.Impl;

public class EnterVentGameEvent : GameEventBase
{
    public EnterVentGameEvent(CustomPlayerData player, int ventId) : base(GameEventType.EnterVent, player)
    {
        VentId = ventId;
    }

    public int VentId { get; }
}