using System;
using COG.Config.Impl;
using COG.Modules;
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
        Objects.AddRange(new []
        {
            HudStringPatch.GetOptByType(CustomOption.CustomOption.CustomOptionType.Neutral)
        });
    }
}