using System;
using COG.Utils;

namespace COG.Config.Impl;

public class LanguageConfig : Config
{
    public static LanguageConfig Instance { get; private set; }
    public string MessageForNextPage { get; private set; }
    public string MakePublicMessage { get; private set; }

    public string GeneralSetting { get; private set; }
    public string ImpostorRolesSetting { get; private set; }
    public string NeutralRolesSetting { get; private set; }
    public string CrewmateRolesSetting { get; private set; }
    public string ModifierSetting { get; private set; }

    public string JesterName { get; private set; }
    public string JesterDescription { get; private set; }

    public string Enable { get; private set; }
    public string Disable { get; private set; }

    public string AllowStartMeeting { get; private set; }

    public LanguageConfig() : base(
        "Language",
        DataDirectoryName + "/language.yml",
        new ResourceFile("COG.Resources.InDLL.Config.language.yml")
    )
    {
        try
        {
            MessageForNextPage = YamlReader.GetString("lobby.message-for-next-page")!;
            MakePublicMessage = YamlReader.GetString("lobby.make-public-message")!;

            GeneralSetting = YamlReader.GetString("menu.general")!;
            ImpostorRolesSetting = YamlReader.GetString("menu.impostor")!;
            NeutralRolesSetting = YamlReader.GetString("menu.neutral")!;
            CrewmateRolesSetting = YamlReader.GetString("menu.crewmate")!;
            ModifierSetting = YamlReader.GetString("menu.modifier")!;

            JesterName = YamlReader.GetString("role.jester.name")!;
            JesterDescription = YamlReader.GetString("role.jester.description")!;

            Enable = YamlReader.GetString("option.enable")!;
            Disable = YamlReader.GetString("option.disable")!;

            AllowStartMeeting = YamlReader.GetString("role.global.allow-start-meeting")!;
        }
        catch (NullReferenceException)
        {
            // 找不到项，说明配置文件版本过低
            // 重新加载
            LoadConfig(true);
            Instance = new LanguageConfig();
        }
    }

    static LanguageConfig()
    {
        Instance = new LanguageConfig();
    }
}