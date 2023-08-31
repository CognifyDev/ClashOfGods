using COG.Config.Impl;
using COG.UI.CustomOption;

namespace COG.States;

public static class GlobalCustomOption
{
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
        DebugMode = CustomOption.Create(1, CustomOption.CustomOptionType.General, 
            LanguageConfig.Instance.DebugMode, false, null, true);
    }
}