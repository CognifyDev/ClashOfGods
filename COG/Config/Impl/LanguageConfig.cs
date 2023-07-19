using System;
using COG.Utils;

namespace COG.Config.Impl;

public class LanguageConfig : Config
{
    public static LanguageConfig Instance { get; private set; } = null!;
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
    public string CogOptions { get; private set; }
    public string ReloadConfigs { get; private set; }
    public string Github { get; private set; }

    public string MaxNumMessage { get; private set; }
    public string AllowStartMeeting { get; private set; }
    public string AllowReportDeadBody { get; private set; }

    public string SidebarTextOriginal { get; private set; }
    public string SidebarTextNeutral { get; private set; }
    public string SidebarTextMod { get; private set; }
    public string SidebarTextModifier { get; private set; }
    public string SidebarTextImpostor { get; private set; }
    public string SidebarTextCrewmate { get; private set; }


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
            CogOptions = YamlReader.GetString("option.main.cog-options")!;
            ReloadConfigs = YamlReader.GetString("option.main.reload-configs")!;
            Github = YamlReader.GetString("option.main.github")!;

            MaxNumMessage = YamlReader.GetString("role.global.max-num")!;
            AllowStartMeeting = YamlReader.GetString("role.global.allow-start-meeting")!;
            AllowReportDeadBody = YamlReader.GetString("role.global.allow-report-body")!;

            SidebarTextOriginal = YamlReader.GetString("sidebar-text.original")!;
            SidebarTextNeutral = YamlReader.GetString("sidebar-text.neutral")!;
            SidebarTextMod = YamlReader.GetString("sidebar-text.mod")!;
            SidebarTextModifier = YamlReader.GetString("sidebar-text.modifier")!;
            SidebarTextImpostor = YamlReader.GetString("sidebar-text.impostor")!;
            SidebarTextCrewmate = YamlReader.GetString("sidebar-text.crewmate")!;
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
        LoadLanguageConfig();
    }

    internal static void LoadLanguageConfig()
    {
        Instance = new LanguageConfig();
    }
}