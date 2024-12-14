using Jint;

namespace COG.Plugin.Impl;

public class JsPlugin : IPlugin
{
    private readonly PluginDescription _description;
    
    public bool Enabled { get; set; }
    
    public Engine Engine { get; }

    public JsPlugin(PluginDescription description, Engine engine)
    {
        _description = description;
        Engine = engine;
    }
    
    public PluginDescription GetDescription()
    {
        return _description;
    }

    public void OnLoad()
    {
        Engine.Invoke("onLoad");
    }

    public void OnEnable()
    {
        Engine.Invoke("onEnable");
    }

    public void OnDisable()
    {
        Engine.Invoke("onDisable");
    }

    public bool IsEnabled()
    {
        return Enabled;
    }

    public IPluginManager GetPluginManager()
    {
        return JsPluginManager.GetManager();
    }
}