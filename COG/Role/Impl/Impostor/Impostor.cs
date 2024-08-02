using AmongUs.GameOptions;
using COG.Config.Impl;

namespace COG.Role.Impl.Impostor;

public class Impostor : CustomRole
{
    public Impostor() : base(LanguageConfig.Instance.ImpostorName, Palette.ImpostorRed, CampType.Impostor)
    {
        CanKill = true;
        CanVent = true;
        IsBaseRole = true;
        CanSabotage = true;
        BaseRoleType = RoleTypes.Impostor;
        ShortDescription = LanguageConfig.Instance.ImpostorDescription;
    }

    public override CustomRole NewInstance()
    {
        return new Impostor();
    }
}