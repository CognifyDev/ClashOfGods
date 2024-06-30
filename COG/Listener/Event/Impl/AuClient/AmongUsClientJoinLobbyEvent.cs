namespace COG.Listener.Event.Impl.AuClient;

public class AmongUsClientJoinLobbyEvent : AmongUsClientEvent
{
    public AmongUsClientJoinLobbyEvent(AmongUsClient client, string gameIdString) : base(client)
    {
        GameIdString = gameIdString;
    }

    public string GameIdString { get; }
}