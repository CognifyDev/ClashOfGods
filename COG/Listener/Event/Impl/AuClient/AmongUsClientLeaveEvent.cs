using InnerNet;

namespace COG.Listener.Event.Impl.AuClient;

public class AmongUsClientLeaveEvent : AmongUsClientEvent
{
    public ClientData ClientData;
    public DisconnectReasons Reason;
    
    public AmongUsClientLeaveEvent(AmongUsClient client, ClientData data, DisconnectReasons reason) : base(client)
    {
        ClientData = data;
        Reason = reason;
    }
}