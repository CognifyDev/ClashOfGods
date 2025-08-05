global using Impostor = COG.Role.Impl.Impostor.Impostor;
using AmongUs.GameOptions;

namespace COG.Role.Impl.Impostor;

public class Impostor : CustomRole
{
    public Impostor() : base(false)
    {
        CanKill = true;
        CanVent = true;
        IsBaseRole = true;
        CanSabotage = true;
        BaseRoleType = RoleTypes.Impostor;
    }
}