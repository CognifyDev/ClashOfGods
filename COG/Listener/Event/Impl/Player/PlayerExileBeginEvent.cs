namespace COG.Listener.Event.Impl.Player;

public class PlayerExileBeginEvent : PlayerEvent
{
    public PlayerExileBeginEvent(PlayerControl? player, ExileController controller, ExileController.InitProperties state) : base(player!)
    {
        ExileController = controller;
        ExileState = state;
    }

    /// <summary>
    ///     驱逐控制器
    /// </summary>
    public ExileController ExileController { get; }


    public ExileController.InitProperties ExileState { get; }
}