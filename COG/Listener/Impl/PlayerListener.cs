using COG.Role;
using COG.Utils;

namespace COG.Listener.Impl;

public class PlayerListener : IListener
{
    public bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        var role = killer.GetRoleInstance();
        var targetRole = target.GetRoleInstance();
        if (role == null || targetRole == null) return true;
        if (role.CampType != CampType.Impostor) return false;
        return targetRole.CampType != CampType.Impostor;
    }
}