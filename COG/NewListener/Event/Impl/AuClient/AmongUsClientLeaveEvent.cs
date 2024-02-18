using InnerNet;

namespace COG.NewListener.Event.Impl.AUClient;

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