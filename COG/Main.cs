using BepInEx;
using BepInEx.Unity.IL2CPP;
using COG.Listener;
using COG.Listener.Impl;
using COG.Utils;
using HarmonyLib;

namespace COG;

[BepInAutoPlugin(PluginGuid, PluginName)]
[BepInIncompatibility("com.emptybottle.townofhost")]
[BepInIncompatibility("me.eisbison.theotherroles")]
[BepInIncompatibility("me.yukieiji.extremeroles")]
[BepInIncompatibility("jp.ykundesu.supernewroles")]
[BepInIncompatibility("com.tugaru.TownOfPlus")]
[BepInProcess("Among Us.exe")]
public partial class Main : BasePlugin
{
    public const string PluginName = "Clash Of Gods";
    public const string PluginGuid = "top.cog.clashofgods";
    public const string PluginVersion = "1.0.0";
    public Harmony harmony { get; } = new(PluginGuid);
    public static BepInEx.Logging.ManualLogSource Logger;

    public static Main Instance;
    
    
    /// <summary>
    /// 插件的启动方法
    /// </summary>
    public override void Load()
    {
        Instance = this;
        
        Logger = BepInEx.Logging.Logger.CreateLogSource("ClashOfGods");
        Logger.LogInfo("Loading...");
        
        ConfigManager.Init();
        
        ListenerManager.GetManager().RegisterListeners(new IListener[] { new CommandListener(), new ChatListener(), new GameListener(), new VersionShowerListener() });
        
        harmony.PatchAll();
    }
}