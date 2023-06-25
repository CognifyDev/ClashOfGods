using BepInEx;
using BepInEx.IL2CPP;
using COG.Listener;
using COG.Listener.Impl;
using HarmonyLib;

namespace COG;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInProcess("Among Us.exe")]
public class Main : BasePlugin
{
    public const string PluginName = "Clash of Gods";
    public const string PluginGuid = "top.cog.clashofgods";
    public const string PluginVersion = "1.0.0";
    public Harmony harmony { get; } = new(PluginGuid);
    public static BepInEx.Logging.ManualLogSource Logger;
    
    
    /// <summary>
    /// 插件的启动方法
    /// </summary>
    public override void Load()
    {
        Logger.LogInfo("Loading...");
        Logger = BepInEx.Logging.Logger.CreateLogSource("ClashOfGods");
        harmony.PatchAll();
        
        ListenerManager.GetManager().RegisterListeners(new IListener[] { new CommandListener() });
    }
}