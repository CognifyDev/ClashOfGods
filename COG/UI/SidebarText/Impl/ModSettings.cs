using COG.Config.Impl;
using COG.UI.CustomOption;

namespace COG.UI.SidebarText.Impl;

public class ModSettings : SidebarText
{
    public ModSettings() : base(LanguageConfig.Instance.SidebarTextMod)
    {
    }
    
    public override void ForResult(ref string result)
    {
        Objects.Clear();
        Objects.AddRange(new []
        {
            HudStringPatch.GetOptByType(CustomOption.CustomOption.CustomOptionType.General)
        });
    }
}