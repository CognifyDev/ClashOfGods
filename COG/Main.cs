global using HarmonyLib;
global using Hazel;
global using Object = UnityEngine.Object;
global using GitInfo = ThisAssembly.Git;
using System;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using COG.Command;
using COG.Command.Impl;
using COG.Config.Impl;
using COG.Constant;
using COG.Game.CustomWinner;
using COG.Game.CustomWinner.Winnable;
using COG.Listener;
using COG.Listener.Impl;
using COG.Patch;
using COG.Plugin.Impl;
using COG.Role;
using COG.Role.Impl;
using COG.Role.Impl.Crewmate;
using COG.Role.Impl.Impostor;
using COG.Role.Impl.Neutral;
using COG.Role.Impl.SubRole;
using COG.UI.ClientOption;
using COG.Utils;
using COG.Utils.Version;
using COG.Utils.WinAPI;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using UnityEngine.SceneManagement;
using Mode = COG.Utils.WinAPI.OpenFileDialogue.OpenFileMode;
using COG.Config;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using COG.UI.CustomButton;

namespace COG;

[BepInAutoPlugin(PluginGuid, PluginName)]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class Main : BasePlugin
{
    public const string PluginName = "Clash Of Gods";
    public const string PluginGuid = "top.cog.clashofgods";
    public const string DisplayName = "ClashOfGods";

    public static StackTraceLogger Logger { get; private set; } = null!;
    public static VersionInfo VersionInfo { get; private set; } = null!;
    public static string PluginVersion { get; private set; } = null!;
    public static DateTime CommitTime => DateTime.Parse(GitInfo.CommitDate);
    private Harmony Harmony { get; } = new(PluginGuid);

    public static Main Instance { get; private set; } = null!;

    /// <summary>
    ///     插件的启动方法
    /// </summary>
    public override void Load()
    {
        Instance = this;

        PluginVersion = ProjectUtils.GetProjectVersion() ?? "Unknown";
        VersionInfo = PluginVersion.Equals("Unknown")
            ? VersionInfo.Empty
            : VersionInfo.Parse(PluginVersion);

        System.Console.OutputEncoding = System.Text.Encoding.UTF8;

        Logger = new StackTraceLogger($"   {DisplayName}");
        Logger.LogInfo("Loading...");
        Logger.LogInfo("Mod Version => " + PluginVersion);
        Logger.LogInfo($"GitInfo: {GitInfo.Branch} ({GitInfo.Commit} at {GitInfo.CommitDate})");

        ResourceUtils.WriteToFileFromResource(
                @".\BepInEx\core\Jint.dll",
            ResourceUtils.GetResourcePath("Libraries.Jint.dll")
            );
        ResourceUtils.WriteToFileFromResource(
                @".\BepInEx\core\YamlDotNet.dll",
            ResourceUtils.GetResourcePath("Libraries.YamlDotNet.dll")
            );
        ResourceUtils.WriteToFileFromResource(
                @".\BepInEx\core\Acornima.dll",
            ResourceUtils.GetResourcePath("Libraries.Acornima.dll")
            );

        var longVersionInfo = StringUtils.EncodeToBase64($"{VersionInfo}{GitInfo.Branch}{GitInfo.Sha}");
        var storagedInfo = "";

        try
        {
            storagedInfo = File.ReadAllText(@$".\{ConfigBase.DataDirectoryName}\VersionInfo.dat");
        }
        catch { }

        ConfigBase.AutoReplace = true;

        if (!storagedInfo.IsNullOrEmptyOrWhiteSpace())
        {
            if (storagedInfo != longVersionInfo)
                Logger.LogWarning("Current mod version doesnt equal to version of last mod running on this machine. Schedule to replace config files...");
            else
                ConfigBase.AutoReplace = false;
        }

        // Read configs by calling static constructors
        _ = SettingsConfig.Instance;
        _ = ButtonHotkeyConfig.Instance;

        File.WriteAllText(@$".\{ConfigBase.DataDirectoryName}\VersionInfo.dat", longVersionInfo);
        
        /*
        ModUpdater.FetchUpdate();
        Logger.LogInfo(
            $"Latest Version => {(Equals(ModUpdater.LatestVersion, VersionInfo.Empty) ? "Unknown" : ModUpdater.LatestVersion!.ToString())}");
        */

        ListenerManager.GetManager().RegisterListeners(new IListener[]
        {
            new CommandListener(),
            new PlayerListener(),
            new DeadPlayerListener(),
            new CustomButtonListener(),
            new GameListener(),
            new ClientOptionListener(),
            new RpcListener(),
            new TaskAdderListener(),
            new VersionShowerListener(),
            new VanillaBugFixListener(),
            new CustomWinnerListener(),
            new LobbyListener(),
            new RoleAssignmentListener(),
            new IntroListener()
        });
        
        // Register CustomWinners
        CustomWinnerManager.GetManager().RegisterCustomWinnables(new IWinnable[]
        {
            new CrewmatesCustomWinner(),
            new ImpostorsCustomWinner(),
            new LastPlayerCustomWinner()
        });

        // Register roles
        CustomRoleManager.GetManager().RegisterRoles(new CustomRole[]
        {
            // Unknown
            new Unknown(),

            // Crewmate
            new Crewmate(),
            new Bait(),
            new Sheriff(),
            new Vigilante(),
            new SoulHunter(),
            new Technician(),
            new Inspector(),
            new Doorman(),
            new Chief(),
            new Enchanter(),

            // Impostor
            new Impostor(),
            new Cleaner(),
            new Stabber(),
            new Reaper(),
            new Troublemaker(),
            new Nightmare(),
            new Spy(),

            // Neutral
            new Jester(),
            new Reporter(),
            new DeathBringer(),

            // Sub-roles
            new Guesser(),
            new SpeedBooster()
        });

        // Register mod options
        ClientOptionManager.GetManager().RegisterClientOptions(new ClientOption[]
        {
            new(LanguageConfig.Instance.LoadCustomLanguage,
                () =>
                {
                    if (GameStates.InRealGame || GameStates.InLobby)
                    {
                        GameUtils.Popup?.Show("You're trying to load the custom language in the game.\nIt may occur some unexpected glitches.\nPlease leave to reload.");
                        return false;
                    }
                    var p = OpenFileDialogue.Display(Mode.Open, "",
                        defaultDir: @$"{Directory.GetCurrentDirectory()}\{ConfigBase.DataDirectoryName}");
                    if (p.FilePath.IsNullOrEmptyOrWhiteSpace()) return false;

                    LanguageConfig.LoadLanguageConfig(p.FilePath!);
                    DestroyableSingleton<OptionsMenuBehaviour>.Instance.Close();
                    SceneManager.LoadScene(Constants.MAIN_MENU_SCENE);
                    return false;
                }, false),
            new(LanguageConfig.Instance.UnloadModButtonName,
                () =>
                {
                    DestroyableSingleton<OptionsMenuBehaviour>.Instance.Close();
                    if (GameStates.InRealGame || GameStates.InLobby)
                    {
                        GameUtils.Popup?.Show(LanguageConfig.Instance.UnloadModInGameErrorMsg);
                        return false;
                    }

                    Unload();
                    GameUtils.Popup?.Show(LanguageConfig.Instance.UnloadModSuccessfulMessage);
                    return false;
                }, false),
            new(LanguageConfig.Instance.HotkeySettingName,
                () =>
                {
                    ClientOption.Buttons.ForEach(o => o.ToggleButton!.gameObject.SetActive(false));
                    ClientOptionListener.HotkeyButtons.ForEach(o => o.SetActive(true));
                    return false;
                }, false)
        });

        // Register Commands
        CommandManager.GetManager().RegisterCommands(new CommandBase[]
        {
            new RpcCommand(),
            new OptionCommand(),
            new DebugCommand()
        });
        
        Harmony.PatchAll();
        
        // Start to load plugins
        if (SettingsConfig.Instance.EnablePluginSystem)
        {
            if (!Directory.Exists(JsPluginManager.PluginDirectoryPath)) Directory.CreateDirectory(JsPluginManager.PluginDirectoryPath);
            
            var files = Directory.GetFiles(JsPluginManager.PluginDirectoryPath).Where(name => name.ToLower().EndsWith(".cog"));
            var enumerable = files.ToArray();
            Logger.LogInfo($"{enumerable.Length} plugin(s) to load.");

            foreach (var file in enumerable)
                JsPluginManager.GetManager().LoadPlugin(file);
        }
        
        _ = GlobalCustomOptionConstant.DebugMode; //调用静态构造函数
    }

    public override bool Unload()
    {
        if (SettingsConfig.Instance.EnablePluginSystem)
            JsPluginManager.GetManager().GetPlugins().ForEach(plugin => plugin.GetPluginManager().UnloadPlugin(plugin));

        // 卸载插件时候，卸载一切东西
        CommandManager.GetManager().GetCommands().Clear();
        ClientOptionManager.GetManager().GetOptions().Clear();
        CustomRoleManager.GetManager().GetRoles().Clear();
        ListenerManager.GetManager().UnregisterHandlers();
        EndGameResult.CachedWinners?.Clear();
        Harmony.UnpatchAll();
        MainMenuPatch.Buttons.Where(b => b).ToList().ForEach(b => b.gameObject.Destroy());
        MainMenuPatch.CustomBanner!.Destroy();
        return false;
    }
}