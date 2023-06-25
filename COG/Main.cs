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
    
    
    /// <summary>
    /// 插件的启动方法
    /// </summary>
    public override void Load()
    {
        harmony.PatchAll();
        
        ListenerManager.GetManager().RegisterListeners(new IListener[] { new CommandListener() });
    }
}