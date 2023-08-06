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
using COG.UI.ModOption;
using COG.UI.SidebarText;
using COG.UI.SidebarText.Impl;
using COG.Utils;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;
using UnityEngine;
using COG.UI.CustomWinner;

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
    public const string PluginVersion = "1.0.0-BETA";
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

        Logger = BepInEx.Logging.Logger.CreateLogSource($"   {DisplayName}");
        Logger.LogInfo("Loading...");

        // Add depends to core directory
        ResourceUtils.WriteToFileFromResource(
            "BepInEx/core/YamlDotNet.dll",
            "COG.Resources.InDLL.Depends.YamlDotNet.dll");
        ResourceUtils.WriteToFileFromResource(
            "BepInEx/core/YamlDotNet.xml",
            "COG.Resources.InDLL.Depends.YamlDotNet.xml");

        var disabledVersion = WebUtils.GetWeb("https://among-us.top/disabledVersions").Split("|");
        if (disabledVersion.Any(s => PluginVersion.Equals(s)))
        {
            Logger.LogError("The version of the mod has been disabled!");
            return;
        }

        BetaVersion = PluginVersion.ToLower().Contains("beta") || PluginVersion.ToLower().Contains("dev");
        if (BetaVersion)
        {
            // 开始验证
            var url = "https://among-us.top/hwids";
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
            new CustomWinnerListener()
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
                }, false)
        });

        // Register Commands
        CommandManager.GetManager().RegisterCommands(new Command.Command[]
        {
            new RpcCommand()
        });

        Harmony.PatchAll();
    }
}