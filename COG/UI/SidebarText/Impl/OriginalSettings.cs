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

        string scientistName = TranslationController.Instance.GetString(StringNames.ScientistRole);
        int idx = result.IndexOf(scientistName);

        if (idx != -1) result = result[..idx] + LanguageConfig.Instance.VanillaRoleDisabled.Color(Palette.ImpostorRed);

        Objects.AddRange(new[]
        {
            result
        });
    }
}