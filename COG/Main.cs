using BepInEx;
using BepInEx.Unity.IL2CPP;
using COG.Listener;
using COG.Listener.Impl;
using COG.Role.Impl;
using HarmonyLib;
using Reactor;

namespace COG;

[BepInAutoPlugin(PluginGuid, PluginName)]
[BepInIncompatibility("com.emptybottle.townofhost")]
[BepInIncompatibility("me.eisbison.theotherroles")]
[BepInIncompatibility("me.yukieiji.extremeroles")]
[BepInIncompatibility("jp.ykundesu.supernewroles")]
[BepInIncompatibility("com.tugaru.TownOfPlus")]
[BepInDependency(ReactorPlugin.Id)]
[BepInProcess("Among Us.exe")]
public partial class Main : BasePlugin
{
    public const string PluginName = "Clash Of Gods";
    public const string PluginGuid = "top.cog.clashofgods";
    public const string PluginVersion = "1.0.0";
    public Harmony harmony { get; } = new(PluginGuid);
    public const string DisplayName = "ClashOfGods";

    public static BepInEx.Logging.ManualLogSource Logger;

    public static Main Instance { get; private set; }


    /// <summary>
    /// 插件的启动方法
    /// </summary>
    public override void Load()
    {
        Instance = this;
        
        Logger = BepInEx.Logging.Logger.CreateLogSource("ClashOfGods");
        Logger.LogInfo("Loading...");
        ListenerManager.GetManager().RegisterListeners(new IListener[] { new CommandListener(), new GameListener(), new VersionShowerListener(), new PlayerListener() });
        
        Role.RoleManager.GetManager().RegisterRoles(new Role.Role[]
        {
            new Crewmate(),
            new Impostor(),
            new Jester()
        });
        
        harmony.PatchAll();
    }
}