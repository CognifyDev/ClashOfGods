using AmongUs.GameOptions;
using COG.Role;

namespace COG.Role.Impl.Neutral;

public class Sidekick : Role
{
    public Sidekick() : base("", RoleManager.GetManager().GetTypeRoleInstance<Jackal>().Color, CampType.Neutral, false)
    {
        BaseRoleType = RoleTypes.Crewmate;
    }
}