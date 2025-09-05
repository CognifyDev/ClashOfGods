using COG.Plugin.Impl;

namespace COG.Plugin;

public interface IPluginManager
{
    void EnablePlugin(IPlugin plugin);

    void DisablePlugin(IPlugin plugin);

    IPlugin LoadPlugin(string path);

    void UnloadPlugin(IPlugin plugin);

    IPlugin[] GetPlugins();

    static IPluginManager GetDefaultManager()
    {
        return JsPluginManager.GetManager();
    }
}