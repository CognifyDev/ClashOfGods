using System;
using COG.Utils;

namespace COG.Config.Impl;

public class LanguageConfig : Config
{
    public static LanguageConfig Instance { get; private set; }
    public string MessageForNextPage { get; private set; } = null!;
    public string MakePublicMessage { get; private set; } = null!;

    public string GeneralSetting { get; private set; } = null!;
    public string ImpostorRolesSetting { get; private set; } = null!;
    public string NeutralRolesSetting { get; private set; } = null!;
    public string CrewmateRolesSetting { get; private set; } = null!;
    public string ModifierSetting { get; private set; } = null!;

    public string SaveGameConfigs { get; private set; } = null!;

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
            MessageForNextPage = GetString("lobby.message-for-next-page");
            MakePublicMessage = GetString("lobby.make-public-message");

            GeneralSetting = GetString("menu.general.name");
            ImpostorRolesSetting = GetString("menu.impostor.name");
            NeutralRolesSetting = GetString("menu.neutral.name");
            CrewmateRolesSetting = GetString("menu.crewmate.name");
            ModifierSetting = GetString("menu.modifier.name");

            SaveGameConfigs = GetString("menu.general.save-game-configs");

            JesterName = GetString("role.jester.name");
            JesterDescription = GetString("role.jester.description");

            Enable = GetString("option.enable");
            Disable = GetString("option.disable");
            CogOptions = GetString("option.main.cog-options");
            ReloadConfigs = GetString("option.main.reload-configs");
            Github = GetString("option.main.github");
            QQ = GetString("option.main.qq");
            Discord = GetString("option.main.discord");

            MaxNumMessage = GetString("role.global.max-num");
            AllowStartMeeting = GetString("role.global.allow-start-meeting");
            AllowReportDeadBody = GetString("role.global.allow-report-body");

            SidebarTextOriginal = GetString("sidebar-text.original");
            SidebarTextNeutral = GetString("sidebar-text.neutral");
            SidebarTextMod = GetString("sidebar-text.mod");
            SidebarTextModifier = GetString("sidebar-text.modifier");
            SidebarTextImpostor = GetString("sidebar-text.impostor");
            SidebarTextCrewmate = GetString("sidebar-text.crewmate");
        }
        catch (NullReferenceException)
        {
            // 找不到项，说明配置文件版本过低
            // 重新加载
            LoadConfig(true);
            Instance = new LanguageConfig();
        }
    }

    private string GetString(string location)
    {
        var toReturn = YamlReader.GetString(location);
        if (toReturn == null)
        {
            throw new NullReferenceException();
        }

        return toReturn;
    }

    static LanguageConfig()
    {
        Instance = new LanguageConfig();
        LoadLanguageConfig();
    }

    internal static void LoadLanguageConfig()
    {
        Instance.LoadConfig(true);
        Instance = new LanguageConfig();
    }
}