using AmongUs.GameOptions;
using COG.Role;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils.Coding;

namespace COG.Role.Impl.Neutral;

[NotUsed]
[NotTested]
public class Jackal : Role
{
    private CustomOption SidekickCanCreateSidekick { get; }
    private CustomOption CreateSidekickCd { get; }
    private CustomOption JackalKillCd { get; }
    private CustomButton CreateSidekickButton { get; }
    private static PlayerControl? CurrentTarget { get; set; }
    public Jackal() : base("Jackal", new(0, 180f, 235f), CampType.Neutral, true)
    {
        Description = "";
        BaseRoleType = RoleTypes.Engineer;

        if (ShowInOptions)
        {
            SidekickCanCreateSidekick = CustomOption.Create(false, CustomOption.CustomOptionType.Neutral, "", true, MainRoleOption);
            CreateSidekickCd = CustomOption.Create(false, CustomOption.CustomOptionType.Neutral, "", 30f, 10f, 60f, 5f, MainRoleOption)!;
            JackalKillCd = CustomOption.Create(false, CustomOption.CustomOptionType.Neutral, "", 30f, 10f, 60f, 5f, MainRoleOption)!;
        }
    }
}