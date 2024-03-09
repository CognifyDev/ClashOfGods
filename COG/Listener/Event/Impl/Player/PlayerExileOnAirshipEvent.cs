namespace COG.Listener.Event.Impl.Player;

public class PlayerExileOnAirshipEvent : PlayerEvent
{
    public ExileController Controller { get; }

    public PlayerExileOnAirshipEvent(PlayerControl player, ExileController controller) : base(player)
    {
        Controller = controller;
    }
}