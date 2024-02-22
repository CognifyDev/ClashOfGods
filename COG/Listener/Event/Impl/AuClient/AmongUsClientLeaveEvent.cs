using InnerNet;

namespace COG.Listener.Event.Impl.AuClient;

public class AmongUsClientLeaveEvent : AmongUsClientEvent
{
    public ClientData ClientData { get; }
    public DisconnectReasons Reason { get; }

    public AmongUsClientLeaveEvent(AmongUsClient client, ClientData data, DisconnectReasons reason) : base(client)
    {
        ClientData = data;
        Reason = reason;
    }
}