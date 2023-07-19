using COG.Utils;

namespace COG.Config.Impl;

/*
 * 暂时不用
 */
public class SettingsConfig : Config
{
    public SettingsConfig() : base(
        "Settings", 
        DataDirectoryName + "/settings.yml",
        new ResourceFile("COG.Resources.InDLL.Config.settings.yml")
    )
    {
        
    }

    static SettingsConfig()
    {
        new SettingsConfig();
    }
}