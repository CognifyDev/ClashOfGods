using InnerNet;

namespace COG.NewListener.Event.Impl.AUClient;

public class AmongUsClientJoinEvent : AmongUsClientEvent
{
    public ClientData ClientData;
    
    public AmongUsClientJoinEvent(AmongUsClient client, ClientData data) : base(client)
    {
        ClientData = data;
    }
}