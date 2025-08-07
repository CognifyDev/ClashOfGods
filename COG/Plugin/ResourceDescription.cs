using System.Collections.Generic;

namespace COG.Plugin;

public class ResourceDescription
{
    private static readonly Dictionary<string, ResourceDescription> Descriptions = new();

    public ResourceDescription(string path, PluginDescription pluginDescription, ResourceType resourceType)
    {
        Path = path;
        PluginDescription = pluginDescription;
        ResourceType = resourceType;

        Descriptions.Add(path, this);
    }

    public string Path { get; }
    public PluginDescription PluginDescription { get; }
    public ResourceType ResourceType { get; }

    public static ResourceDescription? GetDescriptionByPath(string path)
    {
        return Descriptions.GetValueOrDefault(path);
    }

    public static ResourceDescription[] GetDescriptionsByPlugin(IPlugin plugin)
    {
        var toReturn = new List<ResourceDescription>();
        var description = plugin.GetDescription();
        foreach (var (_, value) in Descriptions)
            if (value.PluginDescription.Equals(description))
                toReturn.Add(value);

        return toReturn.ToArray();
    }
}

public enum ResourceType
{
    Config,
    Script,
    Resource
}