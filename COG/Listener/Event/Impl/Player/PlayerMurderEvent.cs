namespace COG.Listener.Event.Impl.Player;

/// <summary>
/// 当一个玩家击杀时，会触发这个事件<para></para>
/// 一般情况下，动作执行者不会为null
/// </summary>
public class PlayerMurderEvent : PlayerEvent
{
    /// <summary>
    /// 被击杀的玩家
    /// </summary>
    public PlayerControl Target { get; }

    public PlayerMurderEvent(PlayerControl killer, PlayerControl target) : base(killer)
    {
        Target = target;
    }
}