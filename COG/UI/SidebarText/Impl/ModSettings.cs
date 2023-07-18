using COG.Modules;

namespace COG.UI.SidebarText.Impl;

public class ModSettings : SidebarText
{
    public ModSettings() : base("模组设置")
    {
        Objects.Add(HudStringPatch.GetOptByType(CustomOption.CustomOptionType.General));
    }
}