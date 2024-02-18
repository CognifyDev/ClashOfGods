using InnerNet;

namespace COG.NewListener.Event.Impl.AUClient;

public class AmongUsClientCreatePlayerEvent : AmongUsClientEvent
{
    public ClientData ClientData { get; }
    
    public AmongUsClientCreatePlayerEvent(AmongUsClient client, ClientData data) : base(client)
    {
        ClientData = data;
    }
}