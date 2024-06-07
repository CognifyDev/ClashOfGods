namespace COG.Listener.Event.Impl.Player;

/// <summary>
///     当一个玩家变形的时候，这个事件会被触发
/// </summary>
public class PlayerShapeShiftEvent : PlayerEvent
{
    public PlayerShapeShiftEvent(PlayerControl player, PlayerControl target, bool hasAnimate) : base(player)
    {
        Target = target;
        HasAnimate = hasAnimate;
    }

    /// <summary>
    ///     被变形的对象
    /// </summary>
    public PlayerControl Target { get; }

    /// <summary>
    ///     是否有动画
    /// </summary>
    public bool HasAnimate { get; }
}