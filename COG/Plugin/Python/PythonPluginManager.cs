using COG.Plugin.Utils;
using COG.Utils.Version;

namespace COG.Plugin.Python;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Scripting.Hosting;
using COG.Utils;

public class PythonPluginManager : IPluginManager
{
    private readonly Dictionary<string, Plugin> _loadedPlugins = new();
    private readonly ScriptEngine _sharedEngine;
    private readonly string _pluginsFolder;
    private readonly string _runtimeCacheFolder;

    public PythonPluginManager(string pluginsFolder)
    {
        _pluginsFolder = pluginsFolder;
        _runtimeCacheFolder = Path.Combine(pluginsFolder, "runtime_cache");
        
        _sharedEngine = IronPython.Hosting.Python.CreateEngine();
        
        try
        {
            if (Directory.Exists(_runtimeCacheFolder))
                Directory.Delete(_runtimeCacheFolder, true);
        }
        catch (Exception ex)
        {
            Main.Logger.LogWarning($"[PluginManager] Failed to clean runtime cache: {ex.Message}. Attempting to continue...");
        }
        Directory.CreateDirectory(_runtimeCacheFolder);
    }
    
    public void LoadAllPlugins()
    {
        var zipFiles = Directory.GetFiles(_pluginsFolder, "*.ca")
            .Concat(Directory.GetFiles(_pluginsFolder, "*.zip"))
            .Distinct()
            .ToList();
        var unsortedDescriptions = new List<PluginInfoContainer>();

        Main.Logger.LogInfo($"Found {zipFiles.Count} plugins(s) to load.");

        foreach (var zipPath in zipFiles)
        {
            try
            {
                var container = PreparePlugin(zipPath);
                unsortedDescriptions.Add(container);
            }
            catch (Exception ex)
            {
                Main.Logger.LogInfo($"Failed to parse plugin at {zipPath}: {ex.Message}");
            }
        }

        // 2. sort
        List<PluginDescription> sortedDescriptions;
        try
        {
            sortedDescriptions = PluginDependencyUtils.SortPlugins(unsortedDescriptions.Select(c => c.Description));
        }
        catch (Exception ex)
        {
            Main.Logger.LogInfo($"Dependency error: {ex.Message}");
            return;
        }

        // 3. load & init
        foreach (var container in sortedDescriptions.Select(desc => unsortedDescriptions.First(c => c.Description.Name == desc.Name)))
        {
            LoadPluginFromContainer(container);
        }
    }

    private record PluginInfoContainer(PluginDescription Description, string RootPath, ZipArchive Archive);

    private PluginInfoContainer PreparePlugin(string zipPath)
    {
        var archive = ZipFile.OpenRead(zipPath);
        var yamlEntry = archive.GetEntry("plugin.yml") ?? throw new FileNotFoundException("plugin.yml missing in zip");

        PluginDescription desc;
        using (var stream = yamlEntry.Open())
        using (var reader = new StreamReader(stream))
        {
            var yamlContent = reader.ReadToEnd();
            var yaml = Yaml.LoadFromString(yamlContent);
            desc = PluginDescription.FromYaml(yaml);
        }

        if (VersionInfo.Parse(desc.ApiVersion).IsNewerThan(Main.VersionInfo))
        {
            archive.Dispose();
            throw new Exception($"Plugin {desc.Name} is invented for a higher version of COG, please update your COG!");
        }

        var pluginRuntimePath = Path.Combine(_runtimeCacheFolder, desc.Name);

        if (Directory.Exists(pluginRuntimePath))
        {
            Directory.Delete(pluginRuntimePath, true);
        }

        foreach (var entry in archive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name)) continue;

            var destPath = Path.GetFullPath(Path.Combine(pluginRuntimePath, entry.FullName));
            var pluginRuntimeFull = Path.GetFullPath(pluginRuntimePath);

            if (!destPath.StartsWith(pluginRuntimeFull + Path.DirectorySeparatorChar)
                && destPath != pluginRuntimeFull)
            {
                Main.Logger.LogWarning($"[PluginManager] Skipping entry with suspicious path: {entry.FullName}");
                continue;
            }

            var destDir = Path.GetDirectoryName(destPath);
            if (destDir != null && !Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            entry.ExtractToFile(destPath, overwrite: true);
        }

        return new PluginInfoContainer(desc, pluginRuntimePath, archive);
    }

    private void LoadPluginFromContainer(PluginInfoContainer container)
    {
        var desc = container.Description;
        Main.Logger.LogInfo($"Loading plugin {desc.Name} {desc.Version}...");

        var resourceIO = new PluginResourceIO(desc.Name, container.Archive);

        var scriptsPath = Path.Combine(container.RootPath, "scripts");
        if (!Directory.Exists(scriptsPath))
        {
            Main.Logger.LogInfo($"Plugin {desc.Name} has no 'scripts' folder.");
            var pluginNoScripts = new Plugin(desc, null!, resourceIO);
            _loadedPlugins[desc.Name] = pluginNoScripts;
            return;
        }

        var handler = new PythonPluginHandler(_sharedEngine, scriptsPath, desc.Main, resourceIO);

        handler.LoadMainScript();

        var plugin = new Plugin(desc, handler, resourceIO);
        _loadedPlugins[desc.Name] = plugin;

        handler.OnInitialize();

        Main.Logger.LogInfo($"Plugin {desc.Name} loaded.");
    }
    
    public void LoadPlugin(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Plugin file not found: {path}");
        }

        PluginInfoContainer container;
        try
        {
            container = PreparePlugin(path);
        }
        catch (Exception ex)
        {
            Main.Logger.LogError($"Failed to prepare plugin from {path}: {ex.Message}");
            return;
        }

        var desc = container.Description;

        if (_loadedPlugins.ContainsKey(desc.Name))
        {
            Main.Logger.LogWarning($"Plugin '{desc.Name}' is already loaded. Skipping.");
            return;
        }

        if (desc.Depends != null)
        {
            foreach (var dep in desc.Depends)
            {
                if (_loadedPlugins.ContainsKey(dep)) continue;
                Main.Logger.LogError($"Cannot load '{desc.Name}': Missing dependency '{dep}'.");
                return;
            }
        }

        if (desc.SoftDepends != null)
        {
            foreach (var softDep in desc.SoftDepends)
            {
                if (!_loadedPlugins.ContainsKey(softDep))
                {
                    Main.Logger.LogInfo($"Soft dependency '{softDep}' for '{desc.Name}' not found. Loading anyway.");
                }
            }
        }

        try
        {
            LoadPluginFromContainer(container);
        }
        catch (Exception ex)
        {
            Main.Logger.LogError($"Failed to initialize plugin '{desc.Name}': {ex.Message}");
        }
    }

    public void UnloadPlugin(string name)
    {
        if (_loadedPlugins.TryGetValue(name, out var plugin))
        {
            UnloadPlugin(plugin);
        }
    }

    public void UnloadPlugin(Plugin plugin)
    {
        plugin.PluginHandler.OnShutdown();
        _loadedPlugins.Remove(plugin.PluginDescription.Name);
        Main.Logger.LogInfo($"Plugin {plugin.PluginDescription.Name} unloaded.");
    }

    public Plugin[] GetPlugins()
    {
        return _loadedPlugins.Values.ToArray();
    }

    public Plugin? GetPlugin(string name)
    {
        return _loadedPlugins.GetValueOrDefault(name);
    }
    
    public void DisableAllPlugins()
    {
        var pluginsReversed = _loadedPlugins.Values.Reverse().ToList();
        foreach (var plugin in pluginsReversed)
        {
            UnloadPlugin(plugin);
        }
    }
}