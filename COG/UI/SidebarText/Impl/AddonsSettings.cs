using COG.Config.Impl;
using COG.UI.CustomOption;

namespace COG.UI.SidebarText.Impl;

public class AddonsSettings : SidebarText
{
    public AddonsSettings() : base(LanguageConfig.Instance.SidebarTextAddons)
    {
    }

    public override void ForResult(ref string result)
    {
        Objects.Clear();
        Objects.AddRange(new[]
        {
            HudStringPatch.GetOptByType(CustomOption.CustomOption.OptionPageType.Addons)
        });
    }
}