namespace COG.Listener;

/// <summary>
/// 监听器接口，用来监听游戏中的行为
/// </summary>
public interface IListener
{
    /// <summary>
    /// 当一个玩家被击杀的时候，触发该监听器
    /// </summary>
    /// <param name="killer">击杀玩家的玩家</param>
    /// <param name="target">被击杀的玩家</param>
    /// <returns></returns>
    bool OnPlayerMurder(PlayerControl killer, PlayerControl target) { return true; }
}