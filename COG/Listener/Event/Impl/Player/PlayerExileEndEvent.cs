namespace COG.Listener.Event.Impl.Player;

public class PlayerExileEndEvent : PlayerEvent
{
    /// <summary>
    /// 驱逐控制器
    /// </summary>
    public ExileController ExileController { get; }

    public PlayerExileEndEvent(PlayerControl player, ExileController controller) : base(player)
    {
        ExileController = controller;
    }
}