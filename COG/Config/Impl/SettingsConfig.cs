using COG.Utils;

namespace COG.Config.Impl;

public class SettingsConfig : Config
{
    public static SettingsConfig Instance { get; }
    
    public SettingsConfig() : base("Settings",
        DataDirectoryName + "/settings.yml",
        new ResourceFile("COG.Resources.InDLL.Config.settings.yml"))
    {
        EnablePluginSystem = YamlReader!.GetBool("plugin.enable")!.Value;
        TimeZone = YamlReader!.GetString("plugin.time-zone")!;
        Culture = YamlReader!.GetString("plugin.culture")!;
        Strict = YamlReader!.GetBool("plugin.strict")!.Value;
    }

    public bool EnablePluginSystem { get; } 
    public string TimeZone { get; }
    public string Culture { get; }
    public bool Strict { get; }

    static SettingsConfig()
    {
        Instance = new SettingsConfig();
        Instance.LoadConfig(true);
    }
}