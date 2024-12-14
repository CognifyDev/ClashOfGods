using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using COG.Config.Impl;
using COG.Utils;
using Jint;

namespace COG.Plugin.Impl;

public class JsPluginManager : IPluginManager
{
    private static JsPluginManager? _manager;
    
    public const string PluginDirectoryPath = Config.Config.DataDirectoryName + "\\plugins";

    public static JsPluginManager GetManager() => _manager ??= new JsPluginManager();

    private readonly List<IPlugin> _plugins = new();

    private readonly Dictionary<ResourceDescription, byte[]> _resources = new();
    
    public void EnablePlugin(IPlugin plugin)
    {
        if (plugin is not JsPlugin jsPlugin) return;
        jsPlugin.Enabled = true;
        var description = plugin.GetDescription();
        Main.Logger.LogInfo($"Plugin {description.Name} v{description.Version} has been enabled.");
    }

    public void DisablePlugin(IPlugin plugin)
    {
        if (plugin is not JsPlugin jsPlugin) return;
        jsPlugin.Enabled = false;
        var description = plugin.GetDescription();
        Main.Logger.LogInfo($"Plugin {description.Name} v{description.Version} has been disabled.");
    }

    public IPlugin LoadPlugin(string path)
    {
        using var zip = ZipFile.OpenRead(path);
        var description = GetDescription(zip);
        if (description == null)
        {
            throw new System.Exception($"{path} is not a available plugin!");
        }
            
        foreach (var entry in zip.Entries)
        {
            if (entry.FullName.StartsWith("scripts"))
            {
                _resources.Add(new ResourceDescription(entry.FullName, description, ResourceType.Script), entry.Open().ToBytes());
            } else if (entry.FullName.StartsWith("configs"))
            {
                _resources.Add(new ResourceDescription(entry.FullName, description, ResourceType.Config), entry.Open().ToBytes());
            } else if (entry.FullName.StartsWith("resources"))
            {
                _resources.Add(new ResourceDescription(entry.FullName, description, ResourceType.Resource), entry.Open().ToBytes());
            }
        }

        var mainScript = zip.GetEntry($"scripts/{description.Main.Replace(".", "/")}");
        if (mainScript == null)
        {
            throw new System.Exception($"{path} is not a available plugin!");
        }

        var modules = new Dictionary<string, ZipArchiveEntry>();
        foreach (var module in description.Modules)
        {
            var spilt = module.Split("|");
            var modulePath = spilt[0];
            var moduleName = spilt[1];
            var entry = zip.GetEntry($"scripts/{modulePath.Replace(".", "/")}");
            if (entry == null)
            {
                continue;
            }
            modules.Add(moduleName, entry);
        }

        var engine = new Engine(options =>
        {
            if (SettingsConfig.Instance.AllowAllClr)
            {
                options.AllowClr();
            }

            if (!SettingsConfig.Instance.TimeZone.ToLower().Equals("default"))
            {
                options.LocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById(SettingsConfig.Instance.TimeZone));
            }

            if (!SettingsConfig.Instance.Culture.ToLower().Equals("default"))
            {
                options.Culture(CultureInfo.GetCultureInfo(SettingsConfig.Instance.Culture));
            }
                
            options
                .Strict(SettingsConfig.Instance.Strict)
                .Constraint(new JsWatchDog());
        });
            
        modules.ForEach(module =>
        {
            engine.Modules.Add(module.Key, Encoding.UTF8.GetString(module.Value.Open().ToBytes()));
        });

        engine.Execute(Encoding.UTF8.GetString(mainScript.Open().ToBytes()));
            
        // setup for engine
        engine.SetValue("getResource", new Func<string, byte[]>(s =>
        {
            var descriptionByPath = ResourceDescription.GetDescriptionByPath(s);
            if (descriptionByPath == null) return Array.Empty<byte>();
                
            return _resources.TryGetValue(descriptionByPath, out var value) ? value : Array.Empty<byte>();
        }));

        var plugin = new JsPlugin(description, engine);
        _plugins.Add(plugin);
        Main.Logger.LogInfo($"Plugin {description.Name} v{description.Version} has been loaded.");
        EnablePlugin(plugin);

        return plugin;
    }

    private static PluginDescription? GetDescription(ZipArchive zipArchive)
    {
        var scriptsEntry = zipArchive.GetEntry("scripts");
        var pluginYmlEntry = zipArchive.GetEntry("plugin.yml");
        var resourcesEntry = zipArchive.GetEntry("resources");
        var configsEntry = zipArchive.GetEntry("configs");
        if (scriptsEntry == null || pluginYmlEntry == null ||
            resourcesEntry == null || configsEntry == null)
        {
            return null;
        }

        var yaml = Yaml.LoadFromBytes(pluginYmlEntry.Open().ToBytes());
        var name = yaml.GetString("name");
        var authors = yaml.GetStringList("authors");
        var version = yaml.GetString("version");
        var main = yaml.GetString("main");
        var modules = yaml.GetStringList("modules");
        
        if (name == null || version == null || main == null) return null; // modules and authors are not a must.
        return new PluginDescription(name, version, authors?.ToArray(), main, modules?.ToArray());
    }

    public void UnloadPlugin(IPlugin plugin)
    {
        var description = plugin.GetDescription();
        DisablePlugin(plugin);
        _plugins.Remove(plugin);
        var descriptionsByPlugin = ResourceDescription.GetDescriptionsByPlugin(plugin);
        descriptionsByPlugin.ForEach(resourceDescription => _resources.Remove(resourceDescription));
        
        Main.Logger.LogInfo($"Plugin {description.Name} v{description.Version} has been loaded.");
    }

    public IPlugin[] GetPlugins()
    {
        return _plugins.ToArray();
    }
}