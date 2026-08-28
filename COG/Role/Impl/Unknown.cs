using AmongUs.GameOptions;
using UnityEngine;

namespace COG.Role.Impl;

public class Unknown : CustomRole
{
    public Unknown() : base(Color.white, CampType.Unknown)
    {
        BaseRoleType = RoleTypes.CrewmateGhost;
    }
}