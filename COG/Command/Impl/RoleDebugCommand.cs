using COG.Utils;
using COG.Role;
using COG.Constant;

namespace COG.Command.Impl;

/// <summary>
///     仅用于调试
/// </summary>
public class RoleDebugCommand : CommandBase
{
    public RoleDebugCommand() : base("role")
    {
    }

    public override bool OnExecute(PlayerControl player, string[] args)
    {
        if (!GlobalCustomOptionConstant.DebugMode.GetBool()) 
            return false;

        var role = CustomRoleManager.GetManager().GetRoleByClassName(args[0], true);
        if (role == null || role.IsSubRole) return true;
        player.RpcSetCustomRole(role);
        return false;
    }
}