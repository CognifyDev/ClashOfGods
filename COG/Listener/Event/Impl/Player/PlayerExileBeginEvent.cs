namespace COG.Listener.Event.Impl.Player;

public class PlayerExileBeginEvent : PlayerEvent
{
    public PlayerExileBeginEvent(PlayerControl? player, ExileController controller, NetworkedPlayerInfo? exiled,
        bool tie) : base(player!)
    {
        ExileController = controller;
        Exiled = exiled;
        Tie = tie;
    }

    /// <summary>
    ///     驱逐控制器
    /// </summary>
    public ExileController ExileController { get; }


    /// <summary>
    ///     被驱逐的玩家，如果有玩家被驱逐此项一定不为null<br />
    ///     被驱逐的玩家退出游戏后，<see cref="ExileController.exiled" />的<see cref="NetworkedPlayerInfo.Object" />必定为null
    /// </summary>
    public NetworkedPlayerInfo? Exiled { get; }

    public bool Tie { get; }
}