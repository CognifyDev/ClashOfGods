using COG.Config.Impl;
using COG.Modules;

namespace COG.UI.SidebarText.Impl;

public class CrewmateSettings : SidebarText
{
    public CrewmateSettings() : base(LanguageConfig.Instance.SidebarTextCrewmate)
    {
    }
    
    public override void ForResult(ref string result)
    {
        Objects.Clear();
        Objects.AddRange(new []
        {
            HudStringPatch.GetOptByType(CustomOption.CustomOptionType.Crewmate)
        });
    }
}