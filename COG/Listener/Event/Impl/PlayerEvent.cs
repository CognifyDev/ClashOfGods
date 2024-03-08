namespace COG.Listener.Event.Impl;

/// <summary>
/// 这个事件为关于玩家操作的总事件
/// 所有关于玩家的事件类都是此事件的子类
/// </summary>
public class PlayerEvent : Event
{
    /// <summary>
    /// 动作执行的玩家
    /// </summary>
    public PlayerControl Player { get; }

    public PlayerEvent(PlayerControl player)
    {
        Player = player;
    }
}