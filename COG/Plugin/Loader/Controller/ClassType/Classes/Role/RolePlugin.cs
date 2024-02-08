using COG.Listener;
using COG.Role;
using UnityEngine;

namespace COG.Plugin.Loader.Controller.ClassType.Classes.Role;

public class RolePlugin : COG.Role.Role
{
    public RolePlugin(string name, Color color, CampType campType, bool showInOptions) : base(name, color, campType, showInOptions)
    {
    }

    public override IListener GetListener(PlayerControl player) => IListener.Empty;
}