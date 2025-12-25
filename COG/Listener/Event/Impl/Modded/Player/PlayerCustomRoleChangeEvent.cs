using COG.Role;

namespace COG.Listener.Event.Impl.Modded.Player;

/// <summary>
/// 当本地缓存玩家的职业（本模组添加的职业）被更改的时候触发
/// 只有主职业变更有效
/// </summary>
public class PlayerCustomRoleChangeEvent(PlayerControl player, CustomRole targetRole, CustomRole originRole)
    : PlayerEvent(player)
{
    /// <summary>
    /// 将被设置的职业
    /// </summary>
    public CustomRole TargetRole { get; } = targetRole;

    /// <summary>
    /// 原来的职业
    /// </summary>
    public CustomRole OriginRole { get; } = originRole;
}