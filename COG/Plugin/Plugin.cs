using System;
using System.IO;
using COG.Plugin.API;
using COG.Value;

namespace COG.Plugin;

public class Plugin
{
    public string Code { get; private set; }
    
    public string Name { get; }
    
    public IPluginBase PluginBase { get; private set; }
    
    protected internal Plugin(FileSystemInfo fileInfo)
    {
        Name = fileInfo.Name;
        Code = File.ReadAllText(fileInfo.FullName);

        var engine = new Jint.Engine();
        engine.Execute(Code);
        var plugin = engine.GetValue(ConstantValue.PluginMainClassName).ToObject() as IPluginBase;
        PluginBase = plugin ?? throw new NullReferenceException();
    }
}