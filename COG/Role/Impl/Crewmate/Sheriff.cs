using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.Utils;

namespace COG.Role.Impl.Crewmate;

public class Sheriff : Role, IListener
{
    public Sheriff() : base(LanguageConfig.Instance.SheriffName, UnityEngine.Color.yellow, CampType.Crewmate, true)
    {
        CanVent = false;
        CanKill = true;
        CanSabotage = false;
        BaseRoleType = RoleTypes.Crewmate;
        Description = LanguageConfig.Instance.SheriffDescription;
    }

    public bool OnPlayerMurder(PlayerControl killer, PlayerControl target)
    {
        if (killer == null || target == null) return true;
        if (killer.GetRoleInstance()!.Name.Equals(Name))
        {
            if (target.GetRoleInstance()!.CampType == CampType.Crewmate)
            {
                killer.MurderPlayer(killer);
            }
        }

        return true;
    }

    public override IListener GetListener(PlayerControl player)
    {
        return this;
    }
}