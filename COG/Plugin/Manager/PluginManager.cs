using System.Collections.Generic;
using System.IO;
using COG.Exception.Plugin;
using COG.Plugin.Loader;

namespace COG.Plugin.Manager;

public static class PluginManager
{
    private const string PluginDirectoryPath = Config.Config.DataDirectoryName + "\\plugins";

    private static readonly List<IPlugin> Plugins = new();

    static PluginManager()
    {
        if (!Directory.Exists(PluginDirectoryPath)) Directory.CreateDirectory(PluginDirectoryPath);
    }

    /// <summary>
    ///     Get a new instance of plugin list
    /// </summary>
    /// <returns>The list instance after cloning</returns>
    public static List<IPlugin> GetPlugins()
    {
        return new List<IPlugin>(Plugins);
    }

    /// <summary>
    ///     Load a plugin
    /// </summary>
    /// <param name="path">the path of the plugin</param>
    /// <returns>the result of the plugin loaded</returns>
    public static IPlugin? LoadPlugin(string path)
    {
        try
        {
            IPlugin plugin = new LuaPluginLoader(path);
            Plugins.Add(plugin);
            return plugin;
        }
        catch (CannotLoadPluginException e)
        {
            Main.Logger.LogError(e.Message);
            return null;
        }
    }

    public static void LoadPlugins()
    {
        var files = Directory.GetDirectories(PluginDirectoryPath);
        foreach (var file in files)
        {
            var directoryInfo = new DirectoryInfo(file);
            var plugin = LoadPlugin(file);
            if (plugin != null)
                Main.Logger.LogInfo(
                    $"Plugin {plugin.GetName()} v{plugin.GetVersion()} made by {plugin.GetAuthor()} was successfully loaded.");
            else
                Main.Logger.LogError("Directory " + directoryInfo.Name + " can not be loaded as a plugin.");
        }

        Main.Logger.LogInfo($"{Plugins.Count} plugin{(Plugins.Count <= 1 ? " was" : "s were")} loaded.");
    }
}