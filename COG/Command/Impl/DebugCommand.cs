using System.Linq;
using COG.Constant;
using COG.Role;
using COG.Utils;

namespace COG.Command.Impl;

/// <summary>
///     �����ڵ���
/// </summary>
public class DebugCommand : CommandBase
{
    public DebugCommand() : base("debug")
    {
    }

    public static bool EnableRpcTest { get; private set; }

    public override bool OnExecute(PlayerControl player, string[] args)
    {
        if (!GlobalCustomOptionConstant.DebugMode.GetBool() || args.Length == 0)
            return false;

        switch (args.First())
        {
            case "role":
            {
                var role = CustomRoleManager.GetManager().GetRoleByClassName(args[0], true);
                if (role == null || role.IsSubRole) return false;
                player.RpcSetCustomRole(role);
                break;
            }
            case "rpctest":
            {
                EnableRpcTest = bool.Parse(args[1]);
                break;
            }
        }

        return false;
    }
}