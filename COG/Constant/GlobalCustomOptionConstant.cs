using COG.Config.Impl;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;

namespace COG.Constant;

public static class GlobalCustomOptionConstant
{
    static GlobalCustomOptionConstant()
    {
        DebugMode = CustomOption.Of(CustomOption.TabType.General,
            () => LanguageConfig.Instance.DebugMode, new BoolOptionValueRule(false)).Register();

        MaxSubRoleNumber = CustomOption.Of(CustomOption.TabType.General,
            () => LanguageConfig.Instance.MaxSubRoleNumber,
            new IntOptionValueRule(0, 1, 10, 1)).Register();
        
        MaxNeutralNumber = CustomOption.Of(CustomOption.TabType.General,
            () => LanguageConfig.Instance.MaxNeutralNumber,
            new IntOptionValueRule(0, 1, 10, 1)).Register();
    }

    public static CustomOption DebugMode { get; }
    public static CustomOption MaxSubRoleNumber { get; }
    public static CustomOption MaxNeutralNumber { get; }
}