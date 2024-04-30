using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;

namespace COG.Role.Impl.Impostor;

public class Impostor : Role
{
    public Impostor() : base(LanguageConfig.Instance.ImpostorName, Palette.ImpostorRed, CampType.Impostor, true)
    {
        CanKill = true;
        CanVent = true;
        IsBaseRole = true;
        CanSabotage = true;
        BaseRoleType = RoleTypes.Impostor;
        Description = LanguageConfig.Instance.ImpostorDescription;
    }

    public override IListener GetListener()
    {
        return IListener.EmptyListener;
    }
}