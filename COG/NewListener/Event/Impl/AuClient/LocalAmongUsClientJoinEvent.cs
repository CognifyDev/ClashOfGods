namespace COG.NewListener.Event.Impl.AUClient;

public class LocalAmongUsClientJoinEvent : AmongUsClientEvent
{
    public string GameIdString { get; }
    
    public LocalAmongUsClientJoinEvent(AmongUsClient client, string gameIdString) : base(client)
    {
        GameIdString = gameIdString;
    }
}