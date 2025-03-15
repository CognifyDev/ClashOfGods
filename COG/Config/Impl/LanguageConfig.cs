using System;
using COG.Utils;
using COG.Utils.Coding;

// ReSharper disable All

namespace COG.Config.Impl;

[ShitCode]
public class LanguageConfig : Config
{
    static LanguageConfig()
    {
        LoadLanguageConfig();
    }


    private LanguageConfig() : base(
        "Language",
        DataDirectoryName + "/language.yml",
        new ResourceFile("COG.Resources.InDLL.Config.language.yml")
    )
    {
        SetTranslations();
    }

    private LanguageConfig(string path) : base("Language", path)
    {
        try
        {
            SetTranslations();
        }
        catch
        {
            // ReSharper disable once Unity.NoNullPropagation
            GameUtils.Popup?.Show("An error occurred when loading language from the disk.");
            Instance = new LanguageConfig();
        }
    }
    
    public static Action OnLanguageLoaded { get; set;  } = new(() => { });

    public static LanguageConfig Instance { get; private set; }
    public string MakePublicMessage { get; private set; } = null!;

    public string GeneralHeaderTitle { get; private set; } = null!;
    public string SavePreset { get; private set; } = null!;
    public string LoadPreset { get; private set; } = null!;
    public string DebugMode { get; private set; } = null!;
    public string MaxSubRoleNumber { get; private set; } = null!;
    public string MaxNeutralNumber { get; private set; } = null!;

    // Crewmate
    public string VigilanteMinCrewmateNumber { get; private set; } = null!;

    public string SoulHunterReviveAfter { get; private set; } = null!;

    // Impostor
    public string CleanBodyCooldown { get; private set; } = null!;

    public string ReaperTimeToReduce { get; private set; } = null!;

    // Neutral
    public string ReporterNeededReportTimes { get; private set; } = null!;
    public string DeathBringerNeededPlayerNumber { get; private set; } = null!;
    
    // Sub-roles
    public string GuesserMaxGuessTime { get; private set; } = null!;
    public string GuesserGuessContinuously { get; private set; } = null!;
    public string GuesserGuessEnabledRolesOnly { get; private set; } = null!;
    public string GuesserCanGuessSubRoles { get; private set; } = null!;

    public string SpeedBoosterIncreasingSpeed { get; private set; } = null!;

    public string Enable { get; private set; } = null!;
    public string Disable { get; private set; } = null!;
    public string CogOptions { get; private set; } = null!;
    public string LoadCustomLanguage { get; private set; } = null!;
    public string GitHub { get; private set; } = null!;
    public string UpdateButtonString { get; private set; } = null!;

    public string MaxNumMessage { get; private set; } = null!;
    public string AllowStartMeeting { get; private set; } = null!;
    public string AllowReportDeadBody { get; private set; } = null!;
    public string KillCooldown { get; private set; } = null!;
    public string NoMoreDescription { get; private set; } = null!;
    public string RoleCode { get; private set; } = null!;
    public string MaxUseTime { get; private set; } = null!;

    public string QQ { get; private set; } = null!;
    public string Discord { get; private set; } = null!;

    public string UnknownCamp { get; private set; } = null!;
    public string ImpostorCamp { get; private set; } = null!;
    public string NeutralCamp { get; private set; } = null!;
    public string CrewmateCamp { get; private set; } = null!;
    public string AddonName { get; private set; } = null!;

    public string UnknownCampDescription { get; private set; } = null!;
    public string ImpostorCampDescription { get; private set; } = null!;
    public string NeutralCampDescription { get; private set; } = null!;
    public string CrewmateCampDescription { get; private set; } = null!;

    public string KillAction { get; private set; } = null!;
    public string CleanAction { get; private set; } = null!;
    public string DispatchAction { get; private set; } = null!;
    public string RepairAction { get; private set; } = null!;
    public string StareAction { get; private set; } = null!;

    public string ShowPlayersRolesMessage { get; private set; } = null!;

    public string Alive { get; private set; } = null!;
    public string Disconnected { get; private set; } = null!;
    public string DefaultKillReason { get; private set; } = null!;
    public string UnknownKillReason { get; private set; } = null!;

    public string UnloadModButtonName { get; private set; } = null!;
    public string UnloadModSuccessfulMessage { get; private set; } = null!;
    public string UnloadModInGameErrorMsg { get; private set; } = null!;

    // Update
    public string UpToDate { get; private set; } = null!;
    public string NonCheck { get; private set; } = null!;
    public string FetchedString { get; private set; } = null!;

    public string ImpostorsWinText { get; private set; } = null!;
    public string CrewmatesWinText { get; private set; } = null!;
    public string NeutralsWinText { get; private set; } = null!;

    public string DefaultEjectText { get; private set; } = null!;
    public string AlivePlayerInfo { get; private set; } = null!;
    
    public string SystemMessage { get; private set; } = null!;

    private void SetTranslations()
    {
        MakePublicMessage = GetString("lobby.make-public-message");

        GeneralHeaderTitle = LoadPreset = GetString("game-setting.general.title");
        LoadPreset = GetString("game-setting.general.load-preset");
        SavePreset = GetString("game-setting.general.save-preset");
        DebugMode = GetString("game-setting.general.debug-mode");
        MaxSubRoleNumber = GetString("game-setting.general.max-sub-role-number");
        MaxNeutralNumber = GetString("game-setting.general.max-neutral-number");
        
        // Crewmate
        VigilanteMinCrewmateNumber = GetString("role.crewmate.vigilante.min-crewmate-number");

        SoulHunterReviveAfter = GetString("role.crewmate.soul-hunter.revive-after");

        // Impostors
        CleanBodyCooldown = GetString("role.impostor.cleaner.clean-cd");
        ReaperTimeToReduce = GetString("role.impostor.reaper.time-to-reduce");

        // Neutral
        ReporterNeededReportTimes = GetString("role.neutral.reporter.neededReportTimes");
        DeathBringerNeededPlayerNumber = GetString("role.neutral.death-bringer.neededPlayerNumber");
        
        // Sub-Roles
        GuesserMaxGuessTime = GetString("role.sub-roles.guesser.max-guess-time");
        GuesserGuessContinuously = GetString("role.sub-roles.guesser.guess-continuously");
        GuesserGuessEnabledRolesOnly = GetString("role.sub-roles.guesser.guess-enabled-roles-only");
        GuesserCanGuessSubRoles = GetString("role.sub-roles.guesser.can-guess-sub-roles");

        SpeedBoosterIncreasingSpeed = GetString("role.sub-roles.speed-booster.increasing-speed");

        Enable = GetString("option.enable");
        Disable = GetString("option.disable");
        CogOptions = GetString("option.main.cog-options");
        LoadCustomLanguage = GetString("option.main.load-custom-lang");
        GitHub = GetString("option.main.github");
        QQ = GetString("option.main.qq");
        Discord = GetString("option.main.discord");
        UpdateButtonString = GetString("option.main.update-button-string");

        MaxNumMessage = GetString("role.global.max-num");
        AllowStartMeeting = GetString("role.global.allow-start-meeting");
        AllowReportDeadBody = GetString("role.global.allow-report-body");
        KillCooldown = GetString("role.global.kill-cooldown");
        NoMoreDescription = GetString("role.global.no-more-description");
        RoleCode = GetString("role.global.role-code");
        MaxUseTime = GetString("role.global.max-use-time");

        UnknownCamp = GetString("camp.unknown.name");
        ImpostorCamp = GetString("camp.impostor.name");
        NeutralCamp = GetString("camp.neutral.name");
        CrewmateCamp = GetString("camp.crewmate.name");
        AddonName = GetString("camp.addon");

        UnknownCampDescription = GetString("camp.unknown.description");
        ImpostorCampDescription = GetString("camp.impostor.description");
        NeutralCampDescription = GetString("camp.neutral.description");
        CrewmateCampDescription = GetString("camp.crewmate.description");

        KillAction = GetString("action.kill");
        CleanAction = GetString("action.clean");
        DispatchAction = GetString("action.dispatch");
        RepairAction = GetString("action.repair");
        StareAction = GetString("action.stare");

        ShowPlayersRolesMessage = GetString("game.end.show-players-roles-message");

        Alive = GetString("game.survival-data.alive");
        Disconnected = GetString("game.survival-data.disconnected");
        DefaultKillReason = GetString("game.survival-data.default");
        UnknownKillReason = GetString("game.survival-data.unknown");

        UnloadModButtonName = GetString("option.main.unload-mod.name");
        UnloadModSuccessfulMessage = GetString("option.main.unload-mod.success");
        UnloadModInGameErrorMsg = GetString("option.main.unload-mod.error-in-game");

        UpToDate = GetString("option.main.update.up-to-date");
        NonCheck = GetString("option.main.update.non-check");
        FetchedString = GetString("option.main.update.fetched");

        ImpostorsWinText = GetString("game.end.wins.impostor");
        CrewmatesWinText = GetString("game.end.wins.crewmate");
        NeutralsWinText = GetString("game.end.wins.neutral");

        DefaultEjectText = GetString("game.exile.default");
        AlivePlayerInfo = GetString("game.exile.alive-player-info");

        SystemMessage = GetString("game.chat.system-message");
    }

    private string GetString(string location)
    {
        var toReturn = YamlReader!.GetString(location);
        if (string.IsNullOrWhiteSpace(toReturn))
        {
            Main.Logger.LogError($"Error getting string (location: {location})");
            toReturn = location;
        }

        return toReturn;
    }

    public TextHandler GetHandler(string location) => new(location);

    private static void LoadLanguageConfig()
    {
        Instance = new LanguageConfig();
    }

    internal static void LoadLanguageConfig(string path)
    {
        Instance = new LanguageConfig(path);
    }

    public class TextHandler
    {
        internal TextHandler(string location)
        {
            Location = location;
        }

        public string Location { get; }

        public string GetString(string target)
        {
            return Instance.GetString($"{Location}.{target}");
        }
    }
}