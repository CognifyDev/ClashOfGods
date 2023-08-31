using System;
using COG.Utils;

namespace COG.Config.Impl;

public class LanguageConfig : Config
{
    static LanguageConfig()
    {
        Instance = new LanguageConfig();
        LoadLanguageConfig();
    }


    private LanguageConfig() : base(
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
            AddonsSetting = GetString("menu.addons.name");

            SaveGameConfigs = GetString("menu.general.save-game-configs");
            DebugMode = GetString("menu.general.debug-mode");

            // Unknown
            UnknownName = GetString("role.unknown.name");
            UnknownDescription = GetString("role.unknown.description");

            // Crewmate
            CrewmateName = GetString("role.crewmate.crewmate.name");
            CrewmateDescription = GetString("role.crewmate.crewmate.description");

            BaitName = GetString("role.crewmate.bait.name");
            BaitDescription = GetString("role.crewmate.bait.description");

            SheriffName = GetString("role.crewmate.sheriff.name");
            SheriffDescription = GetString("role.crewmate.sheriff.description");

            // Impostor
            ImpostorName = GetString("role.impostor.impostor.name");
            ImpostorDescription = GetString("role.impostor.impostor.description");

            // Neutral
            JesterName = GetString("role.neutral.jester.name");
            JesterDescription = GetString("role.neutral.jester.description");

            Enable = GetString("option.enable");
            Disable = GetString("option.disable");
            CogOptions = GetString("option.main.cog-options");
            ReloadConfigs = GetString("option.main.reload-configs");
            Github = GetString("option.main.github");
            QQ = GetString("option.main.qq");
            Discord = GetString("option.main.discord");
            BetaVersionRegisteredUserDisplay = GetString("option.main.beta-version-registered-user-display");

            MaxNumMessage = GetString("role.global.max-num");
            AllowStartMeeting = GetString("role.global.allow-start-meeting");
            AllowReportDeadBody = GetString("role.global.allow-report-body");

            SidebarTextOriginal = GetString("sidebar-text.original");
            SidebarTextNeutral = GetString("sidebar-text.neutral");
            SidebarTextMod = GetString("sidebar-text.mod");
            SidebarTextAddons = GetString("sidebar-text.addons");
            SidebarTextImpostor = GetString("sidebar-text.impostor");
            SidebarTextCrewmate = GetString("sidebar-text.crewmate");

            UnknownCamp = GetString("camp.unknown.name");
            ImpostorCamp = GetString("camp.impostor.name");
            NeutralCamp = GetString("camp.neutral.name");
            CrewmateCamp = GetString("camp.crewmate.name");

            UnknownCampDescription = GetString("camp.unknown.description");
            ImpostorCampDescription = GetString("camp.impostor.description");
            NeutralCampDescription = GetString("camp.neutral.description");
            CrewmateCampDescription = GetString("camp.crewmate.description");

            KillAction = GetString("action.kill");

            ShowPlayersRolesMessage = GetString("game.end.show-players-roles-message");

            Alive = GetString("game.survival-data.alive");
            Disconnected = GetString("game.survival-data.disconnected");
            DefaultKillReason = GetString("game.survival-data.default");
            UnknownKillReason = GetString("game.survival-data.unknown");

            UnloadModButtonName = GetString("option.main.unload-mod.name");
            UnloadModSuccessfulMessage = GetString("option.main.unload-mod.success");
            UnloadModInGameErrorMsg = GetString("option.main.unload-mod.error-in-game");
        }
        catch (NullReferenceException)
        {
            // 找不到项，说明配置文件版本过低
            // 重新加载
            LoadConfig(true);
            Instance = new LanguageConfig();
        }
    }

    public static LanguageConfig Instance { get; private set; }
    public string MessageForNextPage { get; private set; } = null!;
    public string MakePublicMessage { get; private set; } = null!;

    public string GeneralSetting { get; private set; } = null!;
    public string ImpostorRolesSetting { get; private set; } = null!;
    public string NeutralRolesSetting { get; private set; } = null!;
    public string CrewmateRolesSetting { get; private set; } = null!;
    public string AddonsSetting { get; private set; } = null!;

    public string SaveGameConfigs { get; private set; } = null!;
    public string DebugMode { get; private set; } = null!;

    // Unknown
    public string UnknownName { get; private set; } = null!;
    public string UnknownDescription { get; private set; } = null!;

    // Crewmate
    public string CrewmateName { get; private set; } = null!;
    public string CrewmateDescription { get; private set; } = null!;

    public string BaitName { get; private set; } = null!;
    public string BaitDescription { get; private set; } = null!;

    public string SheriffName { get; private set; } = null!;
    public string SheriffDescription { get; private set; } = null!;

    // Impostor
    public string ImpostorName { get; private set; } = null!;
    public string ImpostorDescription { get; private set; } = null!;

    // Neutral
    public string JesterName { get; private set; } = null!;
    public string JesterDescription { get; private set; } = null!;

    public string Enable { get; private set; } = null!;
    public string Disable { get; private set; } = null!;
    public string CogOptions { get; private set; } = null!;
    public string ReloadConfigs { get; private set; } = null!;
    public string Github { get; private set; } = null!;
    public string BetaVersionRegisteredUserDisplay { get; private set; } = null!;

    public string MaxNumMessage { get; private set; } = null!;
    public string AllowStartMeeting { get; private set; } = null!;
    public string AllowReportDeadBody { get; private set; } = null!;

    public string SidebarTextOriginal { get; private set; } = null!;
    public string SidebarTextNeutral { get; private set; } = null!;
    public string SidebarTextMod { get; private set; } = null!;
    public string SidebarTextAddons { get; private set; } = null!;
    public string SidebarTextImpostor { get; private set; } = null!;
    public string SidebarTextCrewmate { get; private set; } = null!;
    public string QQ { get; private set; } = null!;
    public string Discord { get; private set; } = null!;

    public string UnknownCamp { get; private set; } = null!;
    public string ImpostorCamp { get; private set; } = null!;
    public string NeutralCamp { get; private set; } = null!;
    public string CrewmateCamp { get; private set; } = null!;

    public string UnknownCampDescription { get; private set; } = null!;
    public string ImpostorCampDescription { get; private set; } = null!;
    public string NeutralCampDescription { get; private set; } = null!;
    public string CrewmateCampDescription { get; private set; } = null!;

    public string KillAction { get; private set; } = null!;

    public string ShowPlayersRolesMessage { get; private set; } = null!;

    public string Alive { get; private set; } = null!;
    public string Disconnected { get; private set; } = null!;
    public string DefaultKillReason { get; private set; } = null!;
    public string UnknownKillReason { get; private set; } = null!;

    public string UnloadModButtonName { get; private set; } = null!;
    public string UnloadModSuccessfulMessage { get; private set; } = null!;
    public string UnloadModInGameErrorMsg { get; private set; } = null!;

    private string GetString(string location)
    {
        var toReturn = YamlReader.GetString(location);
        if (toReturn is null or "" or " ") throw new NullReferenceException();

        return toReturn;
    }

    internal static void LoadLanguageConfig()
    {
        Instance.LoadConfig(true);
        Instance = new LanguageConfig();
    }
}