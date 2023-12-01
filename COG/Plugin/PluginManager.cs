using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace COG.Plugin;

public class PluginManager
{
    private PluginManager() {}

    private readonly List<Plugin> _plugins = new();
    
    private static PluginManager? _manager;

    public static readonly string PluginsDirectory = Config.Config.DataDirectoryName + "/plugins";

    public static PluginManager GetInstance()
    {
        return _manager ??= new PluginManager();
    }

    internal static void Init()
    {
        if (!Directory.Exists(PluginsDirectory))
        {
            Directory.CreateDirectory(PluginsDirectory);
        }
    }

    public List<Plugin> GetPlugins() => _plugins;

    public void LoadPlugin(FileSystemInfo fileInfo)
    {
        if (_plugins.Any(plugin => plugin.Name.Equals(fileInfo.Name)))
        {
            return;
        }

        try
        {
            var plugin = new Plugin(fileInfo);
            _plugins.Add(plugin);
            
            plugin.PluginBase.OnEnable();
            Main.Logger.LogInfo($"Plugin {fileInfo.Name} was successfully loaded.");
        }
        catch
        {
            Main.Logger.LogError($"Can not load file {fileInfo.Name} as plugin.");
        }
    }

    public void DisablePlugin(Plugin plugin)
    {
        try
        {
            plugin.PluginBase.OnDisable();
            _plugins.Remove(plugin);
        }
        catch
        {
            Main.Logger.LogError($"Plugin {plugin.Name} was successfully disabled.");
        }
    }

    public void LoadPluginsFromDirectory()
    {
        var filesNames = Directory.GetFiles(PluginsDirectory);
        foreach (var fileName in filesNames)
        {
            LoadPlugin(new FileInfo(fileName));
        }
    }

    public void UnloadAllPlugins()
    {
        foreach (var plugin in _plugins)
        {
            DisablePlugin(plugin);
        }
    }
}