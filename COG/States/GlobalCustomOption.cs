using COG.Config.Impl;
using COG.UI.CustomOption;

namespace COG.States;

public static class GlobalCustomOption
{
    public static CustomOption LoadPreset { get; private set; } = null!;
    public static CustomOption SavePreset { get; private set; } = null!;
    public static CustomOption DebugMode { get; private set; } = null!;
    
/*
    static GlobalCustomOption()
    {
        DebugMode = CustomOption.Create(1, CustomOption.CustomOptionType.General, 
            LanguageConfig.Instance.DebugMode, false, null, true);
    }
*/

    internal static void Init()
    {
        LoadPreset = CustomOption.Create(-2, CustomOption.CustomOptionType.General, 
            LanguageConfig.Instance.LoadPreset, false, null, true);
        SavePreset = CustomOption.Create(-1, CustomOption.CustomOptionType.General, 
            LanguageConfig.Instance.SavePreset, false, null, true);
        DebugMode = CustomOption.Create(1, CustomOption.CustomOptionType.General, 
            LanguageConfig.Instance.DebugMode, false, null, true);
    }
}