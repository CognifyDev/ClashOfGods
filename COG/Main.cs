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
using COG.UI.CustomButton;
using COG.UI.ModOption;
using COG.Utils;
using COG.Utils.Version;
using COG.Utils.WinAPI;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using UnityEngine.SceneManagement;
using Mode = COG.Utils.WinAPI.OpenFileDialogue.OpenFileMode;
using GameStates = COG.States.GameStates;

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

        Logger = new StackTraceLogger($"   {DisplayName}");
        Logger.LogInfo("Loading...");
        Logger.LogInfo("Mod Version => " + PluginVersion);
        Logger.LogInfo($"GitInfo: {GitInfo.Branch} ({GitInfo.Commit} at {GitInfo.CommitDate})");
#if !DEBUG
        Logger.DisableMethod(typeof(GameListener).GetMethod(nameof(GameListener.SelectRoles)));
        Logger.DisableMethod(typeof(GameListener).GetMethod(nameof(GameListener.OnRpcReceived)));
#endif
        
/*
        ModUpdater.FetchUpdate();
        Logger.LogInfo(
            $"Latest Version => {(Equals(ModUpdater.LatestVersion, VersionInfo.Empty) ? "Unknown" : ModUpdater.LatestVersion!.ToString())}");
*/

/*
        // Load plugins
        try
        {
            PluginManager.LoadPlugins();
        }
        catch (System.Exception e)
        {
            Logger.LogError(e.Message);
        }
*/
        ListenerManager.GetManager().RegisterListeners(new IListener[]
        {
            new CommandListener(),
            new PlayerListener(),
            new DeadPlayerListener(),
            new CustomButtonListener(),
            new GameListener(),
            new ModOptionListener(),
            new RpcListener(),
            new TaskAdderListener(),
            new VersionShowerListener(),
            new VanillaBugFixListener(),
            new CustomWinnerListener()
        });
        
        // Register CustomWinners
        CustomWinnerManager.GetManager().RegisterCustomWinnables(new IWinnable[]
        {
            new CrewmatesCustomWinner(),
            new ImpostorsCustomWinner(),
            new LastPlayerCustomWinner(),
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

            // Impostor
            new Impostor(),
            new Cleaner(),
            new Assassin(),
            new Reaper(),

            // Neutral
            new Jester(),
            new Reporter(),

            // Sub-roles
            new Guesser(),
            new SpeedBooster()
        });
        
        // Register settings
        SettingsConfig.Instance.LoadConfig();

        // Register mod options
        ModOptionManager.GetManager().RegisterModOptions(new ModOption[]
        {
            new(LanguageConfig.Instance.LoadCustomLanguage,
                () =>
                {
                    if (GameStates.InGame)
                    {
                        GameUtils.Popup?.Show("You're trying to load the custom language in the game.\nIt may occur some unexpected glitches.\nPlease leave to reload.");
                        return false;
                    }
                    var p = OpenFileDialogue.Open(Mode.Open, "*",
                        defaultDir: @$"{Directory.GetCurrentDirectory()}\{COG.Config.Config.DataDirectoryName}");
                    if (p.FilePath is null or "") return false;

                    LanguageConfig.LoadLanguageConfig(p.FilePath!);
                    DestroyableSingleton<OptionsMenuBehaviour>.Instance.Close();
                    SceneManager.LoadScene("MainMenu");
                    return false;
                }, false),
            new(LanguageConfig.Instance.UnloadModButtonName,
                () =>
                {
                    DestroyableSingleton<OptionsMenuBehaviour>.Instance.Close();
                    if (GameStates.InGame)
                    {
                        GameUtils.Popup?.Show(LanguageConfig.Instance.UnloadModInGameErrorMsg);
                        return false;
                    }

                    Unload();
                    GameUtils.Popup?.Show(LanguageConfig.Instance.UnloadModSuccessfulMessage);
                    return false;
                }, false)
        });

        // Register Commands
        CommandManager.GetManager().RegisterCommands(new Command.Command[]
        {
            new RpcCommand(),
            new OptionCommand()
        });
        
        // Register custom buttons
        CustomButtonManager.GetManager().RegisterCustomButton(ButtonConstant.KillButton);
        
        Harmony.PatchAll();
        
        // Start to load plugins
        if (SettingsConfig.Instance.EnablePluginSystem)
        {
            if (!Directory.Exists(JsPluginManager.PluginDirectoryPath)) Directory.CreateDirectory(JsPluginManager.PluginDirectoryPath);
            var files = Directory.GetFiles(JsPluginManager.PluginDirectoryPath).Where(name => name.ToLower().EndsWith(".cog"));
            var enumerable = files as string[] ?? files.ToArray();
            Logger.LogInfo($"{enumerable.Length} plugins to load.");
            foreach (var file in enumerable)
            {
                JsPluginManager.GetManager().LoadPlugin(file);
            }
        }
        
        _ = GlobalCustomOptionConstant.DebugMode; //调用静态构造函数
    }

    public override bool Unload()
    {
        if (SettingsConfig.Instance.EnablePluginSystem)
        {
            JsPluginManager.GetManager().GetPlugins().ForEach(plugin => plugin.GetPluginManager().UnloadPlugin(plugin));
        }

        // 卸载插件时候，卸载一切东西
        CommandManager.GetManager().GetCommands().Clear();
        ModOptionManager.GetManager().GetOptions().Clear();
        CustomRoleManager.GetManager().GetRoles().Clear();
        ListenerManager.GetManager().UnregisterHandlers();
        EndGameResult.CachedWinners?.Clear();
        Harmony.UnpatchAll();
        MainMenuPatch.Buttons.Where(b => b).ToList().ForEach(b => b.gameObject.Destroy());
        MainMenuPatch.CustomBG!.Destroy();
        return false;
    }
}