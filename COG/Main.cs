global using Hazel;
global using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using COG.Command;
using COG.Command.Impl;
using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Impl;
using COG.Role.Impl;
using COG.Role.Impl.Crewmate;
using COG.Role.Impl.Impostor;
using COG.Role.Impl.Neutral;
using COG.UI.CustomWinner;
using COG.UI.CustomWinner.Impl;
using COG.UI.ModOption;
using COG.UI.SidebarText;
using COG.UI.SidebarText.Impl;
using COG.Utils;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using COG.Config;
using System.Threading.Tasks;
using System;

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
    public static string PluginVersion { get; private set; } = "Unknown";
    public const string DisplayName = "ClashOfGods";

    public static ManualLogSource Logger = null!;
    public Harmony Harmony { get; } = new(PluginGuid);

    public static List<string> RegisteredBetaUsers { get; private set; } = new();
    public static bool BetaVersion { get; private set; }

    public static Main Instance { get; private set; } = null!;

    /// <summary>
    ///     插件的启动方法
    /// </summary>
    public override void Load()
    {
        Instance = this;
        PluginVersion = ProjectUtils.GetProjectVersion() ?? "Unknown";

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

        var disabledVersion = WebUtils.GetWeb("https://github.moeyy.xyz/https://raw.githubusercontent.com/CognifyDev/.github/main/disabledVersions").Split("|");
        if (disabledVersion.Any(s => PluginVersion.Equals(s)))
        {
            Logger.LogError("The version of the mod has been disabled!");
            return;
        }

        BetaVersion = PluginVersion.ToLower().Contains("beta") || PluginVersion.ToLower().Contains("dev");
        if (BetaVersion)
        {
            // 开始验证
            const string url = "https://github.moeyy.xyz/https://raw.githubusercontent.com/CognifyDev/.github/main/hwids";
            var hwids = WebUtils.GetWeb(url).Split("|");
            RegisteredBetaUsers = new List<string>(hwids);
            var hostHwid = SystemUtils.GetHwid();
            var success = hwids.Any(hwid => hwid.Equals(hostHwid));
            Logger.LogInfo("Local HWID => " + hostHwid);
            if (!success)
            {
                Logger.LogError("Can not verify, please check!");
                return;
            }
        }

        // Register listeners
        ListenerManager.GetManager().RegisterListeners(new IListener[]
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
            new CachedPlayer()
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

            // Neutral
            new Jester()
        });

        // Register mod options
        ModOptionManager.GetManager().RegisterModOptions(new ModOption[]
        {
            new(LanguageConfig.Instance.ReloadConfigs,
                () =>
                {
                    LanguageConfig.LoadLanguageConfig();
                    Application.Quit();
                    return false;
                }, false),
            new(LanguageConfig.Instance.UnloadModButtonName,
                () =>
                {
                    var popup = GameObject.Instantiate(DiscordManager.Instance.discordPopup, Camera.main.transform);
                    var bg = popup.transform.Find("Background").GetComponent<SpriteRenderer>();
                    var size = bg.size;
                    size.x *= 2.5f;
                    bg.size = size;
                    popup.TextAreaTMP.fontSizeMin = popup.TextAreaTMP.fontSizeMax = popup.TextAreaTMP.fontSize;
                    DestroyableSingleton<OptionsMenuBehaviour>.Instance.Close();
                    if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.NotJoined)
                    {
                        popup.Show(LanguageConfig.Instance.UnloadModInGameErrorMsg);
                        return false;
                    }
                    Unload();
                    popup.Show(LanguageConfig.Instance.UnloadModSuccessfulMessage);
                    Task.Delay(TimeSpan.FromSeconds(3));
                    SceneManager.LoadScene("MainMenu");
                    return false;
                }, false)
        });

        // Register Commands
        CommandManager.GetManager().RegisterCommands(new Command.Command[]
        {
            new RpcCommand()
        });

        // Register CustomWinners
        CustomWinnerManager.RegisterCustomWinnersInstances(new ICustomWinner[]
        {
            new CrewmatesCustomWinner(),
            new ImpostorsCustomWinner(),
            new LastPlayerCustomWinner()
        });

        Harmony.PatchAll();
    }

    public override bool Unload()
    {
        // 卸载插件时候，卸载一切东西
        CommandManager.GetManager().GetCommands().Clear();
        ModOptionManager.GetManager().GetOptions().Clear();
        Role.RoleManager.GetManager().GetRoles().Clear();
        ListenerManager.GetManager().GetListeners().Clear();
        SidebarTextManager.GetManager().GetSidebarTexts().Clear();
        CustomWinnerManager.AllWinners.Clear();
        CustomWinnerManager.CustomWinners.Clear();
        PlayerUtils.Players.Clear();
        Harmony.UnpatchAll();
        return false;
    }
}