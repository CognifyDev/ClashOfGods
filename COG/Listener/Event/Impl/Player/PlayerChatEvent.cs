namespace COG.Listener.Event.Impl.Player;

public class PlayerChatEvent : PlayerEvent
{
    public PlayerChatEvent(PlayerControl player, string text) : base(player)
    {
        Text = text;
    }

    public string Text { get; }
}