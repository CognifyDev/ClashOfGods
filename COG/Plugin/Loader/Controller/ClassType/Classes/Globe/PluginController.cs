using NLua;

namespace COG.Plugin.Loader.Controller.ClassType.Classes.Globe;

public class PluginController
{
    private Lua? Lua { get; }
    private readonly IPlugin _plugin;
    
    internal PluginController(Lua lua, IPlugin plugin)
    {
        Lua = lua;
        _plugin = plugin;
    }
    
    public void UnloadCog()
    {
        DestroyableSingleton<OptionsMenuBehaviour>.Instance.Close();
        Main.Instance.Unload();
    }
    
    public string GetAuthor() => _plugin.GetAuthor();

    public string GetVersion() => _plugin.GetVersion();

    public string GetName() => _plugin.GetName();

    public string GetMainClass() => _plugin.GetMainClass();

    public void OnEnable() => _plugin.OnEnable();

    public void OnDisable() => _plugin.OnDisable();
}