namespace COG.Plugin;

public interface IPlugin
{
    PluginDescription GetDescription();

    void OnLoad();

    void OnEnable();

    void OnDisable();

    bool IsEnabled();

    IPluginManager GetPluginManager();
}