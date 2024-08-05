using COG.Config.Impl;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;

namespace COG.Constant;

public static class GlobalCustomOptionConstant
{
    static GlobalCustomOptionConstant()
    {
        DebugMode = CustomOption.Of(CustomOption.TabType.General,
            () => LanguageConfig.Instance.DebugMode, new BoolOptionValueRule(false),
            null);

        MaxSubRoleNumber = CustomOption.Of(CustomOption.TabType.General,
            () => LanguageConfig.Instance.MaxSubRoleNumber,
            new IntOptionValueRule(0, 1, 10, 1), null);
    }

    public static CustomOption DebugMode { get; }
    public static CustomOption MaxSubRoleNumber { get; }
}