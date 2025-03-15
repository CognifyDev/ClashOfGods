using COG.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace COG.Config.Impl;

public class ButtonHotkeyConfig : Config
{
    public static ButtonHotkeyConfig Instance { get; }

    static ButtonHotkeyConfig()
    {
        Instance = new();
    }

    public ButtonHotkeyConfig() : base("Hotkeys", DataDirectoryName + "/hotkeys.yml")
    {
        
    }
}