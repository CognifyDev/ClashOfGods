global using Hazel;
global using HarmonyLib;
using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Impl;
using COG.Modules;
using COG.Role.Impl.Neutral;
using COG.UI.ModOption;
using COG.UI.SidebarText;
using COG.UI.SidebarText.Impl;
using COG.Utils;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;

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
    public const string PluginVersion = "1.0.0";
    public Harmony Harmony { get; } = new(PluginGuid);
    public const string DisplayName = "ClashOfGods";

    public static BepInEx.Logging.ManualLogSource Logger = null!;

    public static Main Instance { get; private set; } = null!;
    
    /// <summary>
    /// 插件的启动方法
    /// </summary>
    public override void Load()
    {
        Instance = this;

        Logger = BepInEx.Logging.Logger.CreateLogSource(DisplayName + "   ");
        Logger.LogInfo("Loading...");

        // Add depends to core directory
        ResourceUtils.WriteToFileFromResource(
            "BepInEx/core/YamlDotNet.dll",
            "COG.Resources.InDLL.Depends.YamlDotNet.dll");
        ResourceUtils.WriteToFileFromResource(
            "BepInEx/core/YamlDotNet.xml",
            "COG.Resources.InDLL.Depends.YamlDotNet.xml");

        // Register listeners
        ListenerManager.GetManager().RegisterListeners(new IListener[]
        {
            new CommandListener(),
            new GameListener(),
            new VersionShowerListener(),
            new PlayerListener(),
            new OptionListener(),
            new ModOptionListener(),
            new CustomButtonListener()
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
            new Jester()
        });

        // Register mod options
        ModOptionManager.GetManager().RegisterModOptions(new ModOption[]
        {
            new(LanguageConfig.Instance.ReloadConfigs,
                () =>
                {
                    LanguageConfig.LoadLanguageConfig();
                    UnityEngine.Application.Quit();
                    return false;
                }, false)
        });

        Harmony.PatchAll();
    }
}