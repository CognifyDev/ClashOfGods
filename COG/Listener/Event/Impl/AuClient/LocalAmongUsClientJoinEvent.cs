namespace COG.Listener.Event.Impl.AuClient;

public class LocalAmongUsClientJoinEvent : AmongUsClientEvent
{
    public LocalAmongUsClientJoinEvent(AmongUsClient client, string gameIdString) : base(client)
    {
        GameIdString = gameIdString;
    }

    public string GameIdString { get; }
}