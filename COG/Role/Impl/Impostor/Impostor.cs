using AmongUs.GameOptions;

namespace COG.Role.Impl.Impostor;

public class Impostor : CustomRole
{
    public Impostor() : base(Palette.ImpostorRed, CampType.Impostor)
    {
        CanKill = true;
        CanVent = true;
        IsBaseRole = true;
        CanSabotage = true;
        BaseRoleType = RoleTypes.Impostor;
    }

    public override CustomRole NewInstance()
    {
        return new Impostor();
    }
}