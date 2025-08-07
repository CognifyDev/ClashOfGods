using COG.Game.Events;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game;

public class EnterVentGameEvent : GameEventBase
{
    public EnterVentGameEvent(CustomPlayerData player, int ventId) : base(GameEventType.EnterVent, player)
    {
        VentId = ventId;
    }

    public int VentId { get; }
}