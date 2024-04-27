using COG.Config.Impl;
using COG.UI.CustomOption;

namespace COG.Constant;

public static class GlobalCustomOptionConstant
{
    public static readonly CustomOption LoadPreset;

    public static readonly CustomOption SavePreset;

    public static readonly CustomOption DebugMode;

    static GlobalCustomOptionConstant()
    {
        LoadPreset = CustomOption.Create(CustomOption.TabType.General,
            LanguageConfig.Instance.LoadPreset, false, null, true, CustomOption.OptionType.Button);
        
        SavePreset = CustomOption.Create(CustomOption.TabType.General,
            LanguageConfig.Instance.SavePreset, false, null, true, CustomOption.OptionType.Button);

        LoadPreset.OnClickIfButton = new((_) => CustomOption.OpenPresetWithDialogue());
        SavePreset.OnClickIfButton = new((_) => CustomOption.SaveOptionWithDialogue());

        DebugMode = CustomOption.Create(CustomOption.TabType.General,
            LanguageConfig.Instance.DebugMode, false, null, true);
    }
}