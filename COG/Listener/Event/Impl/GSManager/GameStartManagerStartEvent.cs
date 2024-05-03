namespace COG.Listener.Event.Impl.GSManager;

/// <summary>
/// 游戏开始倒计时结束后触发
/// </summary>
public class GameStartManagerStartEvent : GameStartManagerEvent
{
    public GameStartManagerStartEvent(GameStartManager manager) : base(manager)
    {
    }
}