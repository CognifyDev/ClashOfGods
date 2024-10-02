using AmongUs.GameOptions;
using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl;

public class Unknown : CustomRole
{
    public Unknown() : base(Color.white, CampType.Unknown)
    {
        BaseRoleType = RoleTypes.CrewmateGhost;
    }

    public override IListener GetListener()
    {
        return IListener.EmptyListener;
    }
}