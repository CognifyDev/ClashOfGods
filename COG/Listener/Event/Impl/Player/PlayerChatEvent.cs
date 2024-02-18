namespace COG.Listener.Event.Impl.Player;

public class PlayerChatEvent : PlayerEvent
{
    public string Text { get; }
    
    public PlayerChatEvent(PlayerControl player, string text) : base(player)
    {
        Text = text;
    }
}