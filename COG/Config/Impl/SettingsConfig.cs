using System;
using COG.Utils;

namespace COG.Config.Impl;

public class SettingsConfig : Config
{
    public bool EnablePlugin { get; private set; }
    
    public SettingsConfig() : base("Settings",
        DataDirectoryName + "/settings.yml",
        new ResourceFile("COG.Resources.InDLL.Config.settings.yml"))
    {
        try
        {
            EnablePlugin = GetBool("plugin.enable");
        }
        catch (NullReferenceException)
        {
            // 找不到项，说明配置文件版本过低
            // 重新加载
            LoadConfig(true);
            Instance = new SettingsConfig();
        }
    }

    private bool GetBool(string location)
    {
        var toReturn = YamlReader.GetString(location);
        if (toReturn is null or "" or " ") throw new NullReferenceException();

        var outValue = bool.TryParse(toReturn, out bool value);
        if (!outValue)
        {
            throw new NullReferenceException();
        }

        return value;
    }
    
    static SettingsConfig()
    {
        Instance = new SettingsConfig();
        LoadSettingsConfig();
    }

    internal static void LoadSettingsConfig()
    {
        Instance.LoadConfig(true);
        Instance = new SettingsConfig();
    }

    public static SettingsConfig Instance { get; private set; }
}