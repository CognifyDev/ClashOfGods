namespace COG.Listener.Event.Impl.AuClient;

public class LocalAmongUsClientJoinEvent : AmongUsClientEvent
{
    public string GameIdString { get; }
    
    public LocalAmongUsClientJoinEvent(AmongUsClient client, string gameIdString) : base(client)
    {
        GameIdString = gameIdString;
    }
}