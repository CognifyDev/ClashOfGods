namespace COG.Listener.Event.Impl;

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