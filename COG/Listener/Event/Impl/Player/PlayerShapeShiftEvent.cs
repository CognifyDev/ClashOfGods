namespace COG.Listener.Event.Impl.Player;

public class PlayerShapeShiftEvent : PlayerEvent
{
    /// <summary>
    /// 被变形的对象
    /// </summary>
    public PlayerControl Target { get; }
    
    /// <summary>
    /// 是否有动画
    /// </summary>
    public bool ShouldAnimate { get; }
    
    public PlayerShapeShiftEvent(PlayerControl player, PlayerControl target, bool shouldAnimate) : base(player)
    {
        Target = target;
        ShouldAnimate = shouldAnimate;
    }
}