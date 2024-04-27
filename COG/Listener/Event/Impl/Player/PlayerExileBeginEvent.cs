namespace COG.Listener.Event.Impl.Player;

public class PlayerExileBeginEvent : PlayerEvent
{
    /// <summary>
    /// 驱逐控制器
    /// </summary>
    public ExileController ExileController { get; }
    public GameData.PlayerInfo? Exiled { get; }
    public bool Tie { get; }

    public PlayerExileBeginEvent(PlayerControl? player, ExileController controller, GameData.PlayerInfo? exiled, bool tie) : base(player!)
    {
        ExileController = controller;
        Exiled = exiled;
        Tie = tie;
    }
}