namespace COG.Listener.Event.Impl.Player;

/// <summary>
///     一个玩家完成任务时，此事件会执行
/// </summary>
public class PlayerTaskFinishEvent : PlayerEvent
{
    public PlayerTaskFinishEvent(PlayerControl player, uint idx) : base(player)
    {
        Index = idx;
    }

    /// <summary>
    ///     完成任务的序号
    /// </summary>
    public uint Index { get; }
}