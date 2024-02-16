using COG.Config.Impl;
using COG.UI.CustomOption;

namespace COG.States;

public static class GlobalCustomOption
{
    public static CustomOption LoadPreset { get; private set; } = null!;
    public static CustomOption SavePreset { get; private set; } = null!;
    public static CustomOption DebugMode { get; private set; } = null!;

    internal static void Init()
    {
        LoadPreset = CustomOption.Create(true, CustomOption.CustomOptionType.General,
            LanguageConfig.Instance.LoadPreset, false, null, true);
        SavePreset = CustomOption.Create(true, CustomOption.CustomOptionType.General,
            LanguageConfig.Instance.SavePreset, false, null, true);
        DebugMode = CustomOption.Create(false, CustomOption.CustomOptionType.General,
            LanguageConfig.Instance.DebugMode, false, null, true);
    }
}