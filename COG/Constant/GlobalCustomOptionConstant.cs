using COG.Config.Impl;
using COG.UI.CustomOption;

namespace COG.Constant;

public static class GlobalCustomOptionConstant
{
    public static readonly CustomOption? DebugMode;
    public static readonly CustomOption? MaxSubRoleNumber;

    static GlobalCustomOptionConstant()
    {
        var loadPreset = CustomOption.Create(CustomOption.TabType.General,
            LanguageConfig.Instance.LoadPreset, false, null, true, CustomOption.OptionType.Button);

        var savePreset = CustomOption.Create(CustomOption.TabType.General,
            LanguageConfig.Instance.SavePreset, false, null, true, CustomOption.OptionType.Button);

        loadPreset.OnClickIfButton = _ => CustomOption.LoadPresetWithDialogue();
        savePreset.OnClickIfButton = _ => CustomOption.SaveOptionWithDialogue();

        DebugMode = CustomOption.Create(CustomOption.TabType.General,
            LanguageConfig.Instance.DebugMode, false, null, true);

        MaxSubRoleNumber = CustomOption.Create(CustomOption.TabType.General, LanguageConfig.Instance.MaxSubRoleNumber
            , 1, 0, 10, 1, null, true);
    }
}