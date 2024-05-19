namespace COG.Listener.Event.Impl.GSManager;

/// <summary>
/// 玩家进入大厅后触发
/// </summary>
public class GameStartManagerStartEvent : GameStartManagerEvent
{
    public GameStartManagerStartEvent(GameStartManager manager) : base(manager)
    {
    }
}