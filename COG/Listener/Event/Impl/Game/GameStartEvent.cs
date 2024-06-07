namespace COG.Listener.Event.Impl.Game;

/// <summary>
///     游戏真正开始（玩家可移动）时触发
/// </summary>
public class GameStartEvent : GameEvent<GameManager>
{
    public GameStartEvent(GameManager obj) : base(obj)
    {
    }
}