using COG.Constant;
using COG.Role;
using COG.Utils;
using System.Linq;

namespace COG.Command.Impl;

/// <summary>
///     仅用于调试
/// </summary>
public class DebugCommand : CommandBase
{
    public static bool EnableRpcTest { get; private set; } = false;

    public DebugCommand() : base("debug")
    {
    }

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