namespace COG.Listener.Event.Impl.Player;

public class PlayerReportDeadBodyEvent : PlayerEvent
{
    public GameData.PlayerInfo? Target { get; }
    
    public PlayerReportDeadBodyEvent(PlayerControl player, GameData.PlayerInfo? target) : base(player)
    {
        Target = target;
    }
}