using COG.UI.CustomButton;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace COG.Config.Impl;

public class ButtonHotkeyConfig : Config
{
    public static ButtonHotkeyConfig Instance { get; private set; }

    static ButtonHotkeyConfig()
    {
        Instance = new();
    }

    public ButtonHotkeyConfig() : base("Hotkeys", DataDirectoryName + "/hotkeys.yml")
    {
        if (Text.IsNullOrWhiteSpace()) SaveConfigs();

        foreach (var button in CustomButtonManager.GetManager().GetButtons())
        {
            var keyName = YamlReader!.GetString(button.Identifier);
            if (keyName.IsNullOrWhiteSpace()) continue;

            if (Enum.TryParse<KeyCode>(keyName, out var keyCode))
                button.Hotkey = keyCode;
        }
    }

    public void SaveConfigs()
    {
        List<string> configurations = new();

        foreach (var button in CustomButtonManager.GetManager().GetButtons())
            configurations.Add($"{button.Identifier}: \"{button.Hotkey}\"");

        File.WriteAllLines(Path, configurations, Encoding.UTF8);

        Instance = new();
    }
}