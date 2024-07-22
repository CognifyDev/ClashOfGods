/*
using COG.Config.Impl;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;

namespace COG.Constant;

public static class GlobalCustomOptionConstant
{
    public static CustomOption DebugMode { get; }
    public static CustomOption MaxSubRoleNumber { get; }

    static GlobalCustomOptionConstant()
    {
        DebugMode = CustomOption.Create(CustomOption.TabType.General,
            () => LanguageConfig.Instance.DebugMode, new BoolOptionValueRule(false), 
            null, true);

        MaxSubRoleNumber = CustomOption.Create(CustomOption.TabType.General,
            () => LanguageConfig.Instance.MaxSubRoleNumber,
            new IntOptionValueRule(0, 1, 10, 1), null, true);
    }
}*/