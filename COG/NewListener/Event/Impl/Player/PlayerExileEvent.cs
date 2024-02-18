namespace COG.NewListener.Event.Impl.Player;

public class PlayerExileEvent : PlayerEvent
{
    /// <summary>
    /// 驱逐控制器
    /// </summary>
    public ExileController ExileController { get; }
    
    public PlayerExileEvent(PlayerControl player, ExileController controller) : base(player)
    {
        ExileController = controller;
    }
}