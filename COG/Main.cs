global using Hazel;
global using HarmonyLib;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using COG.Command;
using COG.Command.Impl;
using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Impl;
using COG.Patch;
using COG.Role.Impl;
using COG.Role.Impl.Crewmate;
using COG.Role.Impl.Impostor;
using COG.Role.Impl.Neutral;
using COG.States;
using COG.UI.ModOption;
using COG.UI.SidebarText;
using COG.UI.SidebarText.Impl;
using COG.Utils;
using COG.Utils.Version;
using InnerNet;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System.IO;
using COG.Game.CustomWinner;
using COG.Game.CustomWinner.Impl;
using COG.NewListener;
using COG.NewListener.Impl;
using COG.Plugin.Manager;
using COG.Utils.WinAPI;
using UnityEngine.SceneManagement;
using Mode = COG.Utils.WinAPI.OpenFileDialogue.OpenFileMode;

namespace COG;

[BepInAutoPlugin(PluginGuid, PluginName)]
[BepInIncompatibility("com.emptybottle.townofhost")]
[BepInIncompatibility("me.eisbison.theotherroles")]
[BepInIncompatibility("me.yukieiji.extremeroles")]
[BepInIncompatibility("jp.ykundesu.supernewroles")]
[BepInIncompatibility("com.tugaru.TownOfPlus")]
[BepInDependency(ReactorPlugin.Id)]
[BepInProcess("Among Us.exe")]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class Main : BasePlugin
{
    public const string PluginName = "Clash Of Gods";
    public const string PluginGuid = "top.cog.clashofgods";
    public const string DisplayName = "ClashOfGods";

    public static ManualLogSource Logger = null!;
    public static VersionInfo VersionInfo { get; set; } = null!;
    public static string PluginVersion { get; private set; } = "Unknown";
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
            : VersionInfo.NewVersionInfoInstanceByString(PluginVersion);

        Logger = BepInEx.Logging.Logger.CreateLogSource($"   {DisplayName}");
        Logger.LogInfo("Loading...");
        Logger.LogInfo("Mod Version => " + PluginVersion);

        // Add depends to core directory
        ResourceUtils.WriteToFileFromResource(
            "BepInEx/core/YamlDotNet.dll",
            "COG.Resources.InDLL.Depends.YamlDotNet.dll");
        ResourceUtils.WriteToFileFromResource(
            "BepInEx/core/YamlDotNet.xml",
            "COG.Resources.InDLL.Depends.YamlDotNet.xml");
        ResourceUtils.WriteToFileFromResource(
            "BepInEx/core/NLua.dll",
            "COG.Resources.InDLL.Depends.NLua.dll");
        ResourceUtils.WriteToFileFromResource(
            "BepInEx/core/KeraLua.dll",
            "COG.Resources.InDLL.Depends.KeraLua.dll");
        ResourceUtils.WriteToFileFromResource(
            "BepInEx/core/KeraLua.xml",
            "COG.Resources.InDLL.Depends.KeraLua.xml");
        ResourceUtils.WriteToFileFromResource(
            "BepInEx/core/lua54.dll",
            "COG.Resources.InDLL.Depends.lua54.dll");

        var disabledVersion = WebUtils
            .GetWeb(
                "https://raw.kkgithub.com/CognifyDev/.github/main/disabledVersions")
            .Split("|");
        if (disabledVersion.Any(s => PluginVersion.Equals(s)))
        {
            Logger.LogError("The version of the mod has been disabled!");
            return;
        }

        ModUpdater.FetchUpdate();
        
        // Load plugins
        try
        {
            PluginManager.LoadPlugins();
        }
        catch (System.Exception e)
        {
            Logger.LogError(e.Message);
        }
        /*
        // Register listeners
        ListenerManager.GetManager().RegisterListeners(new[]
        {
            new CommandListener(),
            new GameListener(),
            new VersionShowerListener(),
            new PlayerListener(),
            new OptionListener(),
            new ModOptionListener(),
            new CustomButtonListener(),
            new RpcListener(),
            new GameObjectListener(),
            new DeadPlayerManager(),
            new CustomWinnerListener(),
            CachedPlayer.GetCachedPlayerListener(),
            new TaskAdderListener()
        });
        */
        
        ListenerManager.GetManager().RegisterListeners(new IListener[]
        {
            new CommandListener(),
            new PlayerListener()
        });
        

        // Register sidebar texts
        SidebarTextManager.GetManager().RegisterSidebarTexts(new SidebarText[]
        {
            new OriginalSettings(),
            new NeutralSettings(),
            new ModSettings(),
            new AddonsSettings(),
            new ImpostorSettings(),
            new CrewmateSettings()
        });

        // Register roles
        Role.RoleManager.GetManager().RegisterRoles(new Role.Role[]
        {
            // Unknown
            new Unknown(),

            // Crewmate
            new Crewmate(),
            new Bait(),
            new Sheriff(),

            // Impostor
            new Impostor(),
            new Cleaner(),
            // new Troublemaker(),
            // 此职业暂时未完成

            // Neutral
            new Jester(),
            new Opportunist()
        });

        // Register mod options
        ModOptionManager.GetManager().RegisterModOptions(new ModOption[]
        {
            new(LanguageConfig.Instance.LoadCustomLanguage,
                () =>
                {
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
                    if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.NotJoined)
                    {
                        GameUtils.Popup!.Show(LanguageConfig.Instance.UnloadModInGameErrorMsg);
                        return false;
                    }

                    Unload();
                    GameUtils.Popup!.Show(LanguageConfig.Instance.UnloadModSuccessfulMessage);
                    return false;
                }, false)
        });

        // Register Commands
        CommandManager.GetManager().RegisterCommands(new Command.Command[]
        {
            new RpcCommand()
        });

        // Register CustomWinners
        CustomWinnerManager.RegisterCustomWinnersInstances(new IWinnable[]
        {
            new CrewmatesCustomWinner(),
            new ImpostorsCustomWinner(),
            new LastPlayerCustomWinner()
        });

        GlobalCustomOption.Init();

        Harmony.PatchAll();

        foreach (var plugin in PluginManager.GetPlugins()) plugin.OnEnable();
    }

    public override bool Unload()
    {
        // 卸载插件时候，卸载一切东西
        CommandManager.GetManager().GetCommands().Clear();
        ModOptionManager.GetManager().GetOptions().Clear();
        Role.RoleManager.GetManager().GetRoles().Clear();
        ListenerManager.GetManager().UnRegisterHandlers();
        SidebarTextManager.GetManager().GetSidebarTexts().Clear();
        CustomWinnerManager.AllWinners.Clear();
        CustomWinnerManager.CustomWinners.Clear();
        Harmony.UnpatchAll();
        MainMenuPatch.Buttons.Where(b => b).ToList().ForEach(b => b.gameObject.Destroy());
        MainMenuPatch.CustomBG!.Destroy();
        PluginSingleton<ReactorPlugin>.Instance.Unload();

        foreach (var plugin in PluginManager.GetPlugins()) plugin.OnDisable();

        return false;
    }
}