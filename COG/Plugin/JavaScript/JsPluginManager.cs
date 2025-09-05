using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text;
using COG.Config;
using COG.Config.Impl;
using COG.Utils;
using Jint;

namespace COG.Plugin.Impl;

public class JsPluginManager : IPluginManager
{
    public const string PluginDirectoryPath = ConfigBase.DataDirectoryName + "\\plugins";
    private static JsPluginManager? _manager;

    private readonly List<IPlugin> _plugins = new();

    private readonly Dictionary<ResourceDescription, byte[]> _resources = new();

    private JsPluginManager()
    {
    }

    public void EnablePlugin(IPlugin plugin)
    {
        if (plugin is not JsPlugin jsPlugin) return;
        jsPlugin.Enabled = true;
        var description = plugin.GetDescription();
        jsPlugin.OnEnable();
        Main.Logger.LogInfo($"Plugin {description.Name} v{description.Version} has been enabled.");
    }

    public void DisablePlugin(IPlugin plugin)
    {
        if (plugin is not JsPlugin jsPlugin) return;
        jsPlugin.Enabled = false;
        var description = plugin.GetDescription();
        jsPlugin.OnDisable();
        Main.Logger.LogInfo($"Plugin {description.Name} v{description.Version} has been disabled.");
    }

    public IPlugin LoadPlugin(string path)
    {
        using var zip = ZipFile.OpenRead(path);
        var description = GetDescription(zip) ?? throw new System.Exception($"{path} is not a available plugin!");

        foreach (var entry in zip.Entries)
        {
            ResourceType type;

            if (entry.FullName.StartsWith("scripts/"))
                type = ResourceType.Script;
            else if (entry.FullName == "plugin.yml")
                type = ResourceType.Config;
            else
                type = ResourceType.Resource;

            using var stream = entry.Open();
            _resources.Add(new ResourceDescription(entry.FullName, description, type), stream.ToBytes());
        }

        var mainScriptPath = $"scripts/{description.Main}";
        var mainScript = GetEntry(zip, mainScriptPath) ??
                         throw new System.Exception($"{path} is not a available plugin!");

        var modules = new Dictionary<string, ZipArchiveEntry>();
        foreach (var module in description.Modules)
        {
            var spilt = module.Split("|");
            var modulePath = spilt[0];
            var moduleName = spilt[1];
            var entry = zip.GetEntry($"scripts/{modulePath}");
            if (entry == null) continue;
            modules.Add(moduleName, entry);
        }

        var engine = new Engine(options =>
        {
            options.AllowClr(AppDomain.CurrentDomain.GetAssemblies()); // Also includes COG assembly
            options.CatchClrExceptions();

            if (!SettingsConfig.Instance.TimeZone.ToLower().Equals("default"))
                options.LocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById(SettingsConfig.Instance.TimeZone));

            if (!SettingsConfig.Instance.Culture.ToLower().Equals("default"))
                options.Culture(CultureInfo.GetCultureInfo(SettingsConfig.Instance.Culture));

            options
                .Strict(SettingsConfig.Instance.Strict)
                .Constraint(new JsWatchDog());
        });

        modules.ForEach(module =>
        {
            using var moduleStream = module.Value.Open();
            engine.Modules.Add(module.Key, Encoding.UTF8.GetString(moduleStream.ToBytes()));
            engine.Modules.Import(module.Key);
        });

        // set up for engine
        engine.SetValue("getResource", new Func<string, byte[]>(s =>
        {
            var descriptionByPath = ResourceDescription.GetDescriptionByPath(s);
            if (descriptionByPath == null) return Array.Empty<byte>();

            return _resources.TryGetValue(descriptionByPath, out var value) ? value : Array.Empty<byte>();
        }));

        engine.SetValue("logger", new PluginLogger(description));

        using var mainScriptStream = mainScript.Open();
        var scripts = _resources.Where(kvp => kvp.Key.ResourceType == ResourceType.Script)
            .Select(kvp => (kvp.Key, Encoding.UTF8.GetString(kvp.Value))).ToList();
        scripts.Add(new ValueTuple<ResourceDescription, string>(
            _resources.Select(kvp => kvp.Key).First(d => d.Path == mainScriptPath),
            Encoding.UTF8.GetString(mainScriptStream.ToBytes())));
        scripts.ForEach(s => engine.Execute(s.Item2, s.Key.Path));

        var plugin = new JsPlugin(description, engine);
        _plugins.Add(plugin);
        plugin.OnLoad();
        Main.Logger.LogInfo($"Plugin {description.Name} v{description.Version} has been loaded.");
        EnablePlugin(plugin);

        return plugin;
    }

    public void UnloadPlugin(IPlugin plugin)
    {
        var description = plugin.GetDescription();
        DisablePlugin(plugin);
        _plugins.Remove(plugin);
        var descriptionsByPlugin = ResourceDescription.GetDescriptionsByPlugin(plugin);
        descriptionsByPlugin.ForEach(resourceDescription => _resources.Remove(resourceDescription));

        Main.Logger.LogInfo($"Plugin {description.Name} v{description.Version} has been unloaded.");
    }

    public IPlugin[] GetPlugins()
    {
        return _plugins.ToArray();
    }

    public static IPluginManager GetManager()
    {
        return _manager ??= new JsPluginManager();
    }

    private static ZipArchiveEntry? GetEntry(ZipArchive zipArchive, string fullName)
    {
        return zipArchive.Entries.FirstOrDefault(entry => entry.FullName.Equals(fullName));
    }

    private static PluginDescription? GetDescription(ZipArchive zipArchive)
    {
        var scriptsEntry = GetEntry(zipArchive, "scripts/");
        var pluginYmlEntry = zipArchive.GetEntry("plugin.yml");
        var resourcesEntry = GetEntry(zipArchive, "resources/");
        if (scriptsEntry == null || pluginYmlEntry == null ||
            resourcesEntry == null)
            return null;

        var yaml = Yaml.LoadFromBytes(pluginYmlEntry.Open().ToBytes());
        var name = yaml.GetString("name");
        var authors = yaml.GetStringList("authors");
        var version = yaml.GetString("version");
        var main = yaml.GetString("main");
        var modules = yaml.GetStringList("modules");

        if (name == null || version == null || main == null) return null; // modules and authors are not a must.
        return new PluginDescription(name, version, authors?.ToArray(), main, modules?.ToArray());
    }
}