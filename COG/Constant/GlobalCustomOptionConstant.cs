using COG.Config.Impl;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;

namespace COG.Constant;

public static class GlobalCustomOptionConstant
{
    static GlobalCustomOptionConstant()
    {
        DebugMode = CustomOption.Of(CustomOption.TabType.General,
            () => LanguageConfig.Instance.GetString("game-setting.general.debug-mode"), new BoolOptionValueRule(false)).Register();

        MaxSubRoleNumber = CustomOption.Of(CustomOption.TabType.General,
            () => LanguageConfig.Instance.GetString("game-setting.general.max-sub-role-number"),
            new IntOptionValueRule(0, 1, 10, 1)).Register();

        MaxNeutralNumber = CustomOption.Of(CustomOption.TabType.General,
            () => LanguageConfig.Instance.GetString("game-setting.general.max-neutral-number"),
            new IntOptionValueRule(0, 1, 10, 1)).Register();
    }

    public static void Init()
    {
    }

    public static CustomOption DebugMode { get; }
    public static CustomOption MaxSubRoleNumber { get; }
    public static CustomOption MaxNeutralNumber { get; }
}