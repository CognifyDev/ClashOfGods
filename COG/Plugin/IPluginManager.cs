namespace COG.Plugin;

public interface IPluginManager
{
    void LoadAllPlugins();
    
    void LoadPlugin(string path);

    void UnloadPlugin(Plugin plugin);

    void UnloadPlugin(string name);

    void DisableAllPlugins();

    Plugin[] GetPlugins();

    Plugin? GetPlugin(string name);
}