using COG.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace COG.Config.Impl;

public class ButtonHotkeyConfig : ConfigBase
{
    public static ButtonHotkeyConfig Instance { get; private set; }

    public const int MaxButtonCount = 5;
    public const string ButtonPrefix = "button";

    private Dictionary<int, KeyCode> _hotkeys = new();

    static ButtonHotkeyConfig()
    {
        Instance = new();
    }

    public ButtonHotkeyConfig() : base("Hotkeys", DataDirectoryName + "/hotkeys.yml")
    {
        if (Text.IsNullOrEmptyOrWhiteSpace()) return;
        ApplyConfigsFromFile();
    }

    public void SetHotkey(int index, KeyCode hotkey)
    {
        _hotkeys[index] = hotkey;
        Main.Logger.LogInfo($"Hotkey being set: {hotkey}");
        SaveConfigs();
    }

    public ImmutableDictionary<int, KeyCode> GetHotkeys() => _hotkeys.ToImmutableDictionary();

    public void SaveConfigs()
    {
        var stringHotkeys = _hotkeys.Select(kvp => $"{ButtonPrefix}{kvp.Key}: {kvp.Value}");

        File.WriteAllLines(Path, stringHotkeys, Encoding.UTF8);
        Text = string.Join("\r\n", stringHotkeys);
    }

    public void ApplyConfigsFromFile()
    {
        for (var i = 1; i < MaxButtonCount + 1; i++)
        {
            var value = YamlReader!.GetString($"{ButtonPrefix}{i}");
            if (value == null) continue;

            if (!Enum.TryParse<KeyCode>(value, out var keyCode))
            {
                Main.Logger.LogWarning($"Invalid hotkey format for button {i}: {value}");
                continue;
            }

            _hotkeys[i] = keyCode;
        }
    }
}