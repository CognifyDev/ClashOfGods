using InnerNet;

namespace COG.Listener.Event.Impl.AuClient;

public class AmongUsClientJoinEvent : AmongUsClientEvent
{
    public ClientData ClientData;
    
    public AmongUsClientJoinEvent(AmongUsClient client, ClientData data) : base(client)
    {
        ClientData = data;
    }
}