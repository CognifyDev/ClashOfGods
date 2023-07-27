using COG.Config.Impl;
using COG.Modules;
using COG.UI.CustomOption;

namespace COG.UI.SidebarText.Impl;

public class ImpostorSettings : SidebarText
{
    public ImpostorSettings() : base(LanguageConfig.Instance.SidebarTextImpostor)
    {
    }
    
    public override void ForResult(ref string result)
    {
        Objects.Clear();
        Objects.AddRange(new []
        {
            HudStringPatch.GetOptByType(CustomOption.CustomOption.CustomOptionType.Impostor)
        });
    }
}