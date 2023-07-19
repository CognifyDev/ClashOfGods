using COG.Config.Impl;

namespace COG.UI.SidebarText.Impl;

public class OriginalSettings : SidebarText
{
    public OriginalSettings() : base(LanguageConfig.Instance.SidebarTextOriginal)
    {
    }

    public override void ForResult(ref string result)
    {
        Objects.Clear();
        Objects.AddRange(new []
        {
            result
        });
    }
}