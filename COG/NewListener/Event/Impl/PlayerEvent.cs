namespace COG.NewListener.Event.Impl;

public class PlayerEvent : Event
{
    public PlayerControl Player;
    
    protected PlayerEvent(PlayerControl player)
    {
        Player = player;
    }
}