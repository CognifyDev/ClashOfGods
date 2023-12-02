using System;
using System.IO;
using COG.Plugin.API;
using COG.Utils;
using COG.Value;
using Jint;

namespace COG.Plugin;

public class Plugin
{
    public string Code { get; private set; }
    
    public string Name { get; }
    
    public PluginBase PluginBase { get; private set; }
    
    protected internal Plugin(FileSystemInfo fileInfo)
    {
        Name = fileInfo.GetNameWithoutExtension();
        Code = File.ReadAllText(fileInfo.FullName);

        var options = new Options();
        options.Strict = false;
        options.Strict(false);
        var engine = new Engine(options);
        
        engine.Execute(Code);
        var plugin = engine.GetValue(ConstantValue.PluginMainClassName).ToObject() as PluginBase;
        PluginBase = plugin ?? throw new NullReferenceException();
    }
}