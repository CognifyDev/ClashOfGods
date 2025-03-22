using COG.UI.CustomButton;
using COG.Utils;
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
        if (Text.IsNullOrEmptyOrWhiteSpace())
        {
            SaveConfigs();
            return;
        }

        ApplyConfigs();
    }

    public void SetHotkey(CustomButton button, KeyCode? hotkey)
    {
        button.Hotkey = hotkey;
        ApplyConfigs();
        SaveConfigs();
    }

    public void SaveConfigs()
    {
        List<string> configurations = new();

        foreach (var button in CustomButtonManager.GetManager().GetButtons())
            configurations.Add($"{button.Identifier}: {button.Hotkey}");

        File.WriteAllLines(Path, configurations, Encoding.UTF8);
        Text = string.Join("\r\n", configurations);
    }

    public void ApplyConfigs()
    {
        foreach (var button in CustomButtonManager.GetManager().GetButtons())
        {
            var keyName = YamlReader!.GetString(button.Identifier);
            if (keyName.IsNullOrEmptyOrWhiteSpace()) continue;

            if (Enum.TryParse<KeyCode>(keyName, out var keyCode))
            {
                var modDefaultKey = button.Hotkey.HasValue ? button.Hotkey.Value.ToString() : "(null)";

                if (modDefaultKey != keyName)
                {
                    button.Hotkey = keyCode;
                    Main.Logger.LogInfo($"Hotkey change from default: {modDefaultKey} => {keyCode}");
                }
            }
        }
    }
}