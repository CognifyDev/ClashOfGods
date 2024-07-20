namespace COG.Listener.Event.Impl.Player;

public class PlayerExileEndOnAirshipEvent : PlayerEvent
{
    public PlayerExileEndOnAirshipEvent(NetworkedPlayerInfo? player, ExileController controller) : base(player!.Object!)
    {
        Exiled = player;
        Controller = controller;
    }

    public NetworkedPlayerInfo? Exiled { get; }
    public ExileController Controller { get; }
}