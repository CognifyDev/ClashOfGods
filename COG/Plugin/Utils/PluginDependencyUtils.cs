namespace COG.Plugin.Utils;

using System.Collections.Generic;
using System.Linq;

public static class PluginDependencyUtils
{
    public static List<PluginDescription> SortPlugins(IEnumerable<PluginDescription> plugins)
    {
        var pluginMap = plugins.ToDictionary(p => p.Name);
        var adjacencyList = new Dictionary<string, HashSet<string>>();
        var descriptions = pluginMap.Values.ToList();

        foreach (var p in descriptions.Where(p => !adjacencyList.ContainsKey(p.Name)))
        {
            adjacencyList[p.Name] = [];
        }

        foreach (var plugin in descriptions)
        {
            if (plugin.Depends != null)
            {
                foreach (var dep in plugin.Depends)
                {
                    if (pluginMap.ContainsKey(dep))
                    {
                        adjacencyList[plugin.Name].Add(dep); 
                    }
                    else
                    {
                        throw new System.Exception($"Missing dependency: {dep} for plugin {plugin.Name}");
                    }
                }
            }

            if (plugin.SoftDepends != null)
            {
                foreach (var dep in plugin.SoftDepends)
                {
                    if (pluginMap.ContainsKey(dep))
                    {
                        adjacencyList[plugin.Name].Add(dep);
                    }
                }
            }

            if (plugin.LoadBefore == null) continue;
            foreach (var target in plugin.LoadBefore)
            {
                if (pluginMap.ContainsKey(target))
                {
                    adjacencyList[target].Add(plugin.Name);
                }
            }
        }

        var sorted = new List<PluginDescription>();
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();

        foreach (var plugin in descriptions)
        {
            Visit(plugin.Name);
        }

        return sorted;

        void Visit(string pluginName)
        {
            if (recursionStack.Contains(pluginName))
                throw new System.Exception($"Circular dependency detected involving {pluginName}");
            
            if (visited.Contains(pluginName))
                return;

            recursionStack.Add(pluginName);

            if (adjacencyList.TryGetValue(pluginName, out var deps))
            {
                foreach (var dep in deps)
                {
                    Visit(dep);
                }
            }

            recursionStack.Remove(pluginName);
            visited.Add(pluginName);
            sorted.Add(pluginMap[pluginName]);
        }
    }
}