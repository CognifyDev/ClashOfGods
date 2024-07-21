using COG.Utils;
using COG.Utils.Coding;

// ReSharper disable All

namespace COG.Config.Impl;

[ShitCode]
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
        SetTranslations();
    }

    private LanguageConfig(string path) : base(
        "Language",
        path
    )
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

    public static LanguageConfig Instance { get; private set; }
    public string MessageForNextPage { get; private set; } = null!;
    public string MakePublicMessage { get; private set; } = null!;
    public string VanillaRoleDisabled { get; private set; } = null!;

    public string GeneralSetting { get; private set; } = null!;
    public string ImpostorRolesSetting { get; private set; } = null!;
    public string NeutralRolesSetting { get; private set; } = null!;
    public string CrewmateRolesSetting { get; private set; } = null!;
    public string AddonsSetting { get; private set; } = null!;
    public string SavePreset { get; private set; } = null!;
    public string LoadPreset { get; private set; } = null!;
    public string DebugMode { get; private set; } = null!;
    public string MaxSubRoleNumber { get; private set; } = null!;

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
    public string SheriffKillCooldown { get; private set; } = null!;

    public string SpyName { get; private set; } = null!;
    public string SpyDescription { get; private set; } = null!;

    public string VigilanteName { get; private set; } = null!;
    public string VigilanteDescription { get; private set; } = null!;

    // Impostor
    public string ImpostorName { get; private set; } = null!;
    public string ImpostorDescription { get; private set; } = null!;

    public string CleanerName { get; private set; } = null!;
    public string CleanerDescription { get; private set; } = null!;
    public string CleanBodyCooldown { get; private set; } = null!;

    public string BountyHunterName { get; private set; } = null!;
    public string BountyHunterDescription { get; private set; } = null!;
    public string BountyHunterDefaultCd { get; private set; } = null!;
    public string BountyHunterRefreshTargetTime { get; private set; } = null!;
    public string BountyHunterHasArrowToTarget { get; private set; } = null!;
    public string BountyHunterKillCorrectCd { get; private set; } = null!;
    public string BountyHunterKillIncorrectCd { get; private set; } = null!;
    public string BountyHunterCantSelectTargetError { get; private set; } = null!;

    public string TroublemakerName { get; private set; } = null!;
    public string TroublemakerDescription { get; private set; } = null!;
    public string TroublemakerDuration { get; private set; } = null!;
    public string TroublemakerCooldown { get; private set; } = null!;

    public string EraserName { get; private set; } = null!;
    public string EraserDescription { get; private set; } = null!;
    public string EraserInitialEraseCd { get; private set; } = null!;
    public string EraserIncreaseCdAfterErasing { get; private set; } = null!;
    public string EraserCanEraseImpostors { get; private set; } = null!;

    // Neutral
    public string JesterName { get; private set; } = null!;
    public string JesterDescription { get; private set; } = null!;

    public string OpportunistName { get; private set; } = null!;
    public string OpportunistDescription { get; private set; } = null!;

    public string JackalName { get; private set; } = null!;
    public string JackalDescription { get; private set; } = null!;
    public string JackalCreateSidekickCd { get; private set; } = null!;
    public string JackalKillCd { get; private set; } = null!;

    public string SidekickName { get; private set; } = null!;
    public string SidekickDescription { get; private set; } = null!;
    public string SidekickCanCreateSidekick { get; private set; } = null!;

    public string VultureName { get; private set; } = null!;
    public string VultureDescription { get; private set; } = null!;
    public string VultureEatCooldown { get; private set; } = null!;
    public string VultureEatenCountToWin { get; private set; } = null!;
    public string VultureHasArrowToBodies { get; private set; } = null!;

    // Sub-roles
    public string GuesserName { get; private set; } = null!;
    public string GuesserDescription { get; private set; } = null!;
    public string GuesserMaxGuessTime { get; private set; } = null!;
    public string GuesserGuessContinuously { get; private set; } = null!;

    public string LighterName { get; private set; } = null!;
    public string LighterDescription { get; private set; } = null!;

    public string LoverName { get; private set; } = null!;
    public string LoverDescription { get; private set; } = null!;
    public string LoverCountOptionName { get; private set; } = null!;
    public string LoversDieTogetherOptionName { get; private set; } = null!;
    public string LoverEnablePrivateChat { get; private set; } = null!;

    public string Enable { get; private set; } = null!;
    public string Disable { get; private set; } = null!;
    public string CogOptions { get; private set; } = null!;
    public string LoadCustomLanguage { get; private set; } = null!;
    public string Github { get; private set; } = null!;
    public string UpdateButtonString { get; private set; } = null!;

    public string MaxNumMessage { get; private set; } = null!;
    public string AllowStartMeeting { get; private set; } = null!;
    public string AllowReportDeadBody { get; private set; } = null!;
    public string KillCooldown { get; private set; } = null!;

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
    public string CleanAction { get; private set; } = null!;
    public string MakeTrouble { get; private set; } = null!;
    public string CreateSidekick { get; private set; } = null!;
    public string EraseAction { get; private set; } = null!;
    public string AnnihilateAction { get; private set; } = null!;
    public string EatAction { get; private set; } = null!;

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
    public string LoverEjectText { get; private set; } = null!;

    public string SystemMessage { get; private set; } = null!;

    private void SetTranslations()
    {
        MessageForNextPage = GetString("lobby.message-for-next-page");
        MakePublicMessage = GetString("lobby.make-public-message");
        VanillaRoleDisabled = GetString("lobby.vanilla-role-disabled");

        GeneralSetting = GetString("menu.general.name");
        ImpostorRolesSetting = GetString("menu.impostor.name");
        NeutralRolesSetting = GetString("menu.neutral.name");
        CrewmateRolesSetting = GetString("menu.crewmate.name");
        AddonsSetting = GetString("menu.addons.name");

        LoadPreset = GetString("menu.general.load-preset");
        SavePreset = GetString("menu.general.save-preset");
        DebugMode = GetString("menu.general.debug-mode");
        MaxSubRoleNumber = GetString("menu.general.max-sub-role-number");

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
        SheriffKillCooldown = GetString("role.crewmate.sheriff.kill-cd");

        SpyName = GetString("role.crewmate.spy.name");
        SpyDescription = GetString("role.crewmate.spy.description");

        VigilanteName = GetString("role.crewmate.vigilante.name");
        VigilanteDescription = GetString("role.crewmate.vigilante.description");

        // Impostors
        ImpostorName = GetString("role.impostor.impostor.name");
        ImpostorDescription = GetString("role.impostor.impostor.description");

        CleanerName = GetString("role.impostor.cleaner.name");
        CleanerDescription = GetString("role.impostor.cleaner.description");
        CleanBodyCooldown = GetString("role.impostor.cleaner.clean-cd");

        TroublemakerName = GetString("role.impostor.troublemaker.name");
        TroublemakerDescription = GetString("role.impostor.troublemaker.description");
        TroublemakerDuration = GetString("role.impostor.troublemaker.menu.duration");
        TroublemakerCooldown = GetString("role.impostor.troublemaker.menu.cd");

        BountyHunterName = GetString("role.impostor.bounty-hunter.name");
        BountyHunterDescription = GetString("role.impostor.bounty-hunter.description");
        BountyHunterDefaultCd = GetString("role.impostor.bounty-hunter.menu.cd");
        BountyHunterRefreshTargetTime = GetString("role.impostor.bounty-hunter.menu.target-refresh-time");
        BountyHunterHasArrowToTarget = GetString("role.impostor.bounty-hunter.menu.has-arrow");
        BountyHunterKillCorrectCd = GetString("role.impostor.bounty-hunter.menu.cd-kill-target");
        BountyHunterKillIncorrectCd = GetString("role.impostor.bounty-hunter.menu.cd-kill-non-target");
        BountyHunterCantSelectTargetError = GetString("role.impostor.bounty-hunter.error.select-target-error");

        EraserName = GetString("role.impostor.eraser.name");
        EraserDescription = GetString("role.impostor.eraser.description");
        EraserInitialEraseCd = GetString("role.impostor.eraser.menu.initial-erase-cd");
        EraserIncreaseCdAfterErasing = GetString("role.impostor.eraser.menu.increase-cd-after-erasing");
        EraserCanEraseImpostors = GetString("role.impostor.eraser.menu.can-erase-imps");

        // Neutral
        JesterName = GetString("role.neutral.jester.name");
        JesterDescription = GetString("role.neutral.jester.description");

        OpportunistName = GetString("role.neutral.opportunist.name");
        OpportunistDescription = GetString("role.neutral.opportunist.description");

        JackalName = GetString("role.neutral.jackal.name");
        JackalDescription = GetString("role.neutral.jackal.description");
        JackalCreateSidekickCd = GetString("role.neutral.jackal.create-sidekick-cd");
        JackalKillCd = GetString("role.neutral.jackal.kill-cd");

        SidekickName = GetString("role.neutral.sidekick.name");
        SidekickDescription = GetString("role.neutral.sidekick.description");
        SidekickCanCreateSidekick = GetString("role.neutral.sidekick.can-create-sidekick");

        VultureName = GetString("role.neutral.vulture.name");
        VultureDescription = GetString("role.neutral.vulture.description");
        VultureEatCooldown = GetString("role.neutral.vulture.eat-cd");
        VultureEatenCountToWin = GetString("role.neutral.vulture.count-to-win");
        VultureHasArrowToBodies = GetString("role.neutral.vulture.has-arrow");

        GuesserDescription = GetString("role.sub-roles.guesser.description");
        GuesserMaxGuessTime = GetString("role.sub-roles.guesser.max-guess-time");
        GuesserGuessContinuously = GetString("role.sub-roles.guesser.guess-continuously");

        LighterName = GetString("role.sub-roles.lighter.name");
        LighterDescription = GetString("role.sub-roles.lighter.description");

        LoverName = GetString("role.sub-roles.lover.name");
        LoverDescription = GetString("role.sub-roles.lover.description");
        LoverCountOptionName = GetString("role.sub-roles.lover.count-option");
        LoversDieTogetherOptionName = GetString("role.sub-roles.lover.die-together");
        LoverEnablePrivateChat = GetString("role.sub-roles.lover.private-chat");

        Enable = GetString("option.enable");
        Disable = GetString("option.disable");
        CogOptions = GetString("option.main.cog-options");
        LoadCustomLanguage = GetString("option.main.load-custom-lang");
        Github = GetString("option.main.github");
        QQ = GetString("option.main.qq");
        Discord = GetString("option.main.discord");
        UpdateButtonString = GetString("option.main.update-button-string");

        MaxNumMessage = GetString("role.global.max-num");
        AllowStartMeeting = GetString("role.global.allow-start-meeting");
        AllowReportDeadBody = GetString("role.global.allow-report-body");
        KillCooldown = GetString("role.global.kill-cooldown");

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
        CleanAction = GetString("action.clean");
        MakeTrouble = GetString("action.make-trouble");
        CreateSidekick = GetString("action.create-sidekick");
        EraseAction = GetString("action.erase-action");
        AnnihilateAction = GetString("action.annihilate");

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
        LoverEjectText = GetString("game.exile.lover-message");

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

    private static void LoadLanguageConfig()
    {
        Instance.LoadConfig(true);
        Instance = new LanguageConfig();
    }

    internal static void LoadLanguageConfig(string path)
    {
        Instance = new LanguageConfig(path);
    }
}