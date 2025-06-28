namespace COG.Listener.Event.Impl.Player;

/// <summary>
///     当一个玩家击杀时，会触发这个事件
///     <para></para>
///     一般情况下，动作执行者不会为null
///     <para></para>
///     当标记为<see cref="EventHandlerType.Prefix"/>时，只有房主会触发
/// </summary>
public class PlayerMurderEvent : PlayerEvent
{
    public PlayerMurderEvent(PlayerControl killer, PlayerControl target) : base(killer)
    {
        Target = target;
    }

    /// <summary>
    ///     被击杀的玩家
    /// </summary>
    public PlayerControl Target { get; }
}