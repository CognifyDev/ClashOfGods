using COG.Config.Impl;
using COG.Modules;

namespace COG.UI.SidebarText.Impl;

public class ModifierSettings : SidebarText
{
    public ModifierSettings() : base(LanguageConfig.Instance.SidebarTextModifier)
    {
    }
    
    public override void ForResult(ref string result)
    {
        Objects.Clear();
        Objects.AddRange(new []
        {
            HudStringPatch.GetOptByType(CustomOption.CustomOptionType.Modifier)
        });
    }
}