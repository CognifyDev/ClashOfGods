using COG.Config.Impl;
using COG.UI.CustomOption;

namespace COG.UI.SidebarText.Impl;

public class NeutralSettings : SidebarText
{
    public NeutralSettings() : base(LanguageConfig.Instance.SidebarTextNeutral)
    {
    }

    public override void ForResult(ref string result)
    {
        Objects.Clear();
        Objects.AddRange(new[]
        {
            HudStringPatch.GetOptByType(CustomOption.CustomOption.OptionPageType.Neutral)
        });
    }
}