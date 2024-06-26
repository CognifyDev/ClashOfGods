namespace COG.Listener.Event.Impl.Player;

public class PlayerExileEndOnAirshipEvent : PlayerEvent
{
    public PlayerExileEndOnAirshipEvent(PlayerControl player, ExileController controller) : base(player)
    {
        Controller = controller;
    }

    public ExileController Controller { get; }
}