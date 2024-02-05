using System.Collections.Generic;
using System.IO;
using COG.Exception.Plugin;
using COG.Plugin.Loader;

namespace COG.Plugin.Manager;

public static class PluginManager
{
    private static readonly string PluginDirectoryPath = Config.Config.DataDirectoryName + "\\plugins";
    
    private static readonly List<IPlugin> Plugins = new();

    static PluginManager()
    {
        if (!Directory.Exists(PluginDirectoryPath))
        {
            Directory.CreateDirectory(PluginDirectoryPath);
        }
    }

    /// <summary>
    /// Get a new instance of plugin list
    /// </summary>
    /// <returns>The list instance after cloning</returns>
    public static List<IPlugin> GetPlugins() => new(Plugins);

    /// <summary>
    /// Load a plugin
    /// </summary>
    /// <param name="path">the path of the plugin</param>
    /// <returns>the result of the plugin loaded</returns>
    public static bool LoadPlugin(string path)
    {
        try
        {
            IPlugin plugin = new LuaPluginLoader(path);
            Plugins.Add(plugin);
            return true;
        }
        catch (CannotLoadPluginException e)
        {
            Main.Logger.LogError(e.Message);
            return false;
        }
    }

    public static void LoadPlugins()
    {
        var files = Directory.GetFiles(PluginDirectoryPath);
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            if (LoadPlugin(file))
            {
                Main.Logger.LogInfo("File " + fileInfo.Name + " was loaded as a plugin successfully.");
            }
            else
            {
                Main.Logger.LogError("File " + fileInfo.Name + " can not be loaded as a plugin.");
            }
        }
        
        Main.Logger.LogInfo($"{Plugins.Count} plugin{(Plugins.Count <= 1 ? " was" : "s were")} loaded.");
    }
}