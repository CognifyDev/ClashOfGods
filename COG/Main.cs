using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;

namespace COG;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInProcess("Among Us.exe")]
public class Main : BasePlugin
{
    
    public const string PluginName = "Cash of God";
    public const string PluginGuid = "top.cog.cashofgod";
    public const string PluginVersion = "1.0.0";
    public Harmony harmony { get; } = new(PluginGuid);
    
    
    /// <summary>
    /// 插件的启动方法
    /// </summary>
    public override void Load()
    {
        harmony.PatchAll();
    }
}