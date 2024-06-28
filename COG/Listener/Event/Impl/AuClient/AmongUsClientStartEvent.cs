using InnerNet;

namespace COG.Listener.Event.Impl.AuClient;

public class AmongUsClientStartEvent : AmongUsClientEvent
{
    public AmongUsClientStartEvent(AmongUsClient client) : base(client)
    {
    }
}