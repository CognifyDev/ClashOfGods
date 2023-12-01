using System.Collections.Generic;
using System.IO;

namespace COG.Plugin;

public class PluginManager
{
    private PluginManager() {}

    private readonly List<Plugin> Plugins = new();
    
    private static PluginManager? _manager = null;

    public static PluginManager GetInstance()
    {
        return _manager ??= new PluginManager();
    }

    public void LoadPlugin(FileInfo fileInfo)
    {
        try
        {
            var plugin = new Plugin(fileInfo);
            Plugins.Add(plugin);
        }
        catch
        {
            Main.Logger.LogError($"Can not load file {fileInfo.Name} as plugin.");
        }
    }
}