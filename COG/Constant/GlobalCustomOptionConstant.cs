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
        LoadPreset = CustomOption.Create(true, CustomOption.CustomOptionType.General,
            LanguageConfig.Instance.LoadPreset, false, null, true);
        
        SavePreset = CustomOption.Create(true, CustomOption.CustomOptionType.General,
            LanguageConfig.Instance.SavePreset, false, null, true);
        
        DebugMode = CustomOption.Create(false, CustomOption.CustomOptionType.General,
            LanguageConfig.Instance.DebugMode, false, null, true);
    }
}