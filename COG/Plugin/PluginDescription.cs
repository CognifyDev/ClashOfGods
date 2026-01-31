using System;
using COG.Utils;

namespace COG.Plugin;

public record PluginDescription(
    string Name,
    string Version,
    string? Description,
    string ApiVersion,
    string Main,
    string[]? Authors,
    string[]? Depends,
    string[]? SoftDepends,
    string? Website,
    string Prefix,
    string[]? LoadBefore)
{
    public static PluginDescription FromYaml(Yaml yaml)
    {
        // required arguments
        var name = yaml.GetString("name");
        var version = yaml.GetString("version");
        var apiVersion = yaml.GetString("api-version");
        var main = yaml.GetString("main");
        object?[] requiredOnes = [name, version, apiVersion, main];
        
        CheckIfNull(requiredOnes);
        
        // nullable arguments
        var description = yaml.GetString("description");
        
        var authorInYaml = yaml.GetString("author");
        var authors = authorInYaml == null ? yaml.GetStringArray("authors") : [authorInYaml];
        
        var depends = yaml.GetStringArray("depend");
        var softDepends = yaml.GetStringArray("soft-depend");
        var website = yaml.GetString("website");
        var prefix = yaml.GetString("prefix") ?? name;
        var loadBefore = yaml.GetStringArray("load-before");

        return new PluginDescription(name!, version!, description, apiVersion!, main!, authors, depends, softDepends,
            website, prefix!, loadBefore);
    }

    static void CheckIfNull(object?[] objects)
    {
        foreach (var o in objects)
        {
            CheckIfNull(o);
        }
    }

    static void CheckIfNull(object? obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }
    }
}