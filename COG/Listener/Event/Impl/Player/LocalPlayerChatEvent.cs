namespace COG.Listener.Event.Impl.Player;

public class LocalPlayerChatEvent : PlayerEvent
{
    public string Text { get; }

    public LocalPlayerChatEvent(PlayerControl sender, string text) : base(sender)
    {
        Text = text;
    }
}