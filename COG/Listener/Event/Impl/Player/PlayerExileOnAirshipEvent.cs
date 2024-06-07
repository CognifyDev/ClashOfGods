namespace COG.Listener.Event.Impl.Player;

public class PlayerExileOnAirshipEvent : PlayerEvent
{
    public PlayerExileOnAirshipEvent(PlayerControl player, ExileController controller) : base(player)
    {
        Controller = controller;
    }

    public ExileController Controller { get; }
}