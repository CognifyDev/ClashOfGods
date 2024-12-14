using System;

namespace COG.Plugin;

public class PluginDescription
{
    public string Name { get; }
    public string Version { get; }
    public string[]? Authors { get; }
    public string Main { get; }
    public string[] Modules { get; }
    
    public PluginDescription(string name, string version, string[]? authors, string main, string[]? modules)
    {
        Name = name;
        Version = version;
        Authors = authors;
        Main = main;
        Modules = modules ?? Array.Empty<string>();
    } 
}