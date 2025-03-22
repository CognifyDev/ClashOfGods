using COG.Utils;
using COG.Role;

namespace COG.Command.Impl;

/// <summary>
///     �����ڵ���<para />
///     �������ue��c#����ִ̨��COG.Command.CommandManager.GetManager().RegisterCommand(new COG.Command.Impl.RoleDebugCommand());
/// </summary>
public class RoleDebugCommand : CommandBase
{
    public RoleDebugCommand() : base("role")
    {
    }

    public override bool OnExecute(PlayerControl player, string[] args)
    {
        var role = CustomRoleManager.GetManager().GetRoleByClassName(args[0], true);
        if (role == null || role.IsSubRole) return true;
        player.RpcSetCustomRole(role);
        return false;
    }
}