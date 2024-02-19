namespace COG.Listener.Event.Impl.Player;

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