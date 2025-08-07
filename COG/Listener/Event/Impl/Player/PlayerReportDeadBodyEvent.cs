namespace COG.Listener.Event.Impl.Player;

/// <summary>
///     当一个玩家启动会议(包括报告尸体、紧急会议)的时候，这个事件会被触发
///     <para />
///     只有房主可以正常触发此Listener
/// </summary>
public class PlayerReportDeadBodyEvent : PlayerEvent
{
    public PlayerReportDeadBodyEvent(PlayerControl player, NetworkedPlayerInfo? target) : base(player)
    {
        Target = target;
    }

    /// <summary>
    ///     如果为报告尸体而启动的会议，那么这一项会是被报告尸体的<see cref="NetworkedPlayerInfo" />
    ///     <para />
    ///     如果是紧急会议，那么这个会为null
    /// </summary>
    public NetworkedPlayerInfo? Target { get; }
}