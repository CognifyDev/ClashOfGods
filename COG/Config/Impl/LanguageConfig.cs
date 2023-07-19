using System;
using COG.Utils;

namespace COG.Config.Impl;

public class LanguageConfig : Config
{
    public static LanguageConfig Instance { get; private set; } = null!;
    public string MessageForNextPage { get; private set; } = null!;
    public string MakePublicMessage { get; private set; } = null!;

    public string GeneralSetting { get; private set; } = null!;
    public string ImpostorRolesSetting { get; private set; } = null!;
    public string NeutralRolesSetting { get; private set; } = null!;
    public string CrewmateRolesSetting { get; private set; } = null!;
    public string ModifierSetting { get; private set; } = null!;

    public string JesterName { get; private set; } = null!;
    public string JesterDescription { get; private set; } = null!;

    public string Enable { get; private set; } = null!;
    public string Disable { get; private set; } = null!;
    public string CogOptions { get; private set; } = null!;
    public string ReloadConfigs { get; private set; } = null!;
    public string Github { get; private set; } = null!;

    public string MaxNumMessage { get; private set; } = null!;
    public string AllowStartMeeting { get; private set; } = null!;
    public string AllowReportDeadBody { get; private set; } = null!;

    public string SidebarTextOriginal { get; private set; } = null!;
    public string SidebarTextNeutral { get; private set; } = null!;
    public string SidebarTextMod { get; private set; } = null!;
    public string SidebarTextModifier { get; private set; } = null!;
    public string SidebarTextImpostor { get; private set; } = null!;
    public string SidebarTextCrewmate { get; private set; } = null!;
    public string QQ { get; private set; } = null!;
    public string Discord { get; private set; } = null!;


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
            QQ = YamlReader.GetString("option.main.qq")!;
            Discord = YamlReader.GetString("option.main.discord")!;

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