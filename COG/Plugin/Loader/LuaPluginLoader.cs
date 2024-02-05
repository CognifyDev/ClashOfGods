using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using COG.Exception.Plugin;
using COG.Plugin.Manager;
using COG.Utils;
using LuaFunction = NLua.LuaFunction;

namespace COG.Plugin.Loader;

[Serializable]
public class LuaPluginLoader : IPlugin
{
    /// <summary>
    /// 插件的名字
    /// </summary>
    private string _name;

    /// <summary>
    /// 插件的作者
    /// </summary>
    private string _author;

    /// <summary>
    /// 插件的版本
    /// </summary>
    private string _version;

    /// <summary>
    /// 插件的主类
    /// </summary>
    private string _mainClass;
    
    private string ScriptPath { get; }
    private NLua.Lua LuaController { get; }
    
    private LuaFunction OnEnableFunction { get; }
    private LuaFunction OnDisableFunction { get; }

    public LuaPluginLoader(string scriptPath)
    {
        ScriptPath = scriptPath;
        var directoryInfo = new DirectoryInfo(ScriptPath);
        
        if (!CheckDirectory(directoryInfo))
            throw new CannotLoadPluginException($"{directoryInfo.Name} not a legal plugin");
        LuaController = new NLua.Lua();

        var pluginYaml = Yaml.LoadFromFile(ScriptPath + "\\plugin.yml");
        try
        {
            _name = pluginYaml.GetString("name")!;
            _author = pluginYaml.GetString("author")!;
            _version = pluginYaml.GetString("version")!;
            _mainClass = pluginYaml.GetString("main")!;
        }
        catch
        {
            throw new CannotLoadPluginException($"{directoryInfo.Name} not a legal plugin");
        }
        
        LuaController.DoFile(ScriptPath + "\\" + _mainClass);
        if (!CheckPlugin()) 
            throw new CannotLoadPluginException($"{directoryInfo.Name} not a correct plugin with functions");

        OnEnableFunction = LuaController.GetFunction("onEnable");
        OnDisableFunction = LuaController.GetFunction("onDisable");
        MakeLanguage();
    }

    private static bool CheckDirectory(FileSystemInfo directoryInfo) =>
        directoryInfo.Exists && File.Exists(directoryInfo.FullName + "\\plugin.yml");

    private bool CheckPlugin()
    {
        var onEnableFunction = LuaController.GetFunction("onEnable");
        var onDisableFunction = LuaController.GetFunction("onDisable");
        return onEnableFunction != null && onDisableFunction != null;
    }

    public void MakeLanguage()
    {
        // register methods
        var functionsType = typeof(Functions);
        foreach (var methodInfo in functionsType.GetMethods())
        {
            var attributes = methodInfo.GetCustomAttributes(typeof(FunctionRegisterAttribute), false);
            if (attributes.Length != 1)
            {
                continue;
            }

            if (attributes[0] is FunctionRegisterAttribute functionRegisterAttribute)
            {
                LuaController.RegisterFunction(functionRegisterAttribute.FunctionName, null, methodInfo);
            }
        }
        
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    private sealed class FunctionRegisterAttribute : Attribute
    {
        public string FunctionName { get; }

        public FunctionRegisterAttribute(string functionName)
        {
            FunctionName = functionName;
        }
    }
    
    private class Functions
    {
        [FunctionRegister("logInfo")]
        public static void Info(string param)
        {
            Main.Logger.LogInfo(param);
        }
        
        [FunctionRegister("logError")]
        public static void Error(string param)
        {
            Main.Logger.LogError(param);
        }
        
        [FunctionRegister("logWarning")]
        public static void Warning(string param)
        {
            Main.Logger.LogWarning(param);
        }
        
        [FunctionRegister("logDebug")]
        public static void Debug(string param)
        {
            Main.Logger.LogDebug(param);
        }

        [FunctionRegister("getAuthor")]
        public static string GetAuthor(string pluginName)
        {
            foreach (var plugin in PluginManager.GetPlugins().Where(plugin => plugin.GetName().Equals(pluginName)))
            {
                return plugin.GetAuthor();
            }

            return "null";
        }
        
        [FunctionRegister("getVersion")]
        public static string GetVersion(string pluginName)
        {
            foreach (var plugin in PluginManager.GetPlugins().Where(plugin => plugin.GetName().Equals(pluginName)))
            {
                return plugin.GetVersion();
            }

            return "null";
        }
        
        [FunctionRegister("getMainClass")]
        public static string GetMainClass(string pluginName)
        {
            foreach (var plugin in PluginManager.GetPlugins().Where(plugin => plugin.GetName().Equals(pluginName)))
            {
                return plugin.GetMainClass();
            }

            return "null";
        }
    }
    
    public void OnEnable()
    {
        OnEnableFunction.Call();
    }

    public void OnDisable()
    {
        OnDisableFunction.Call();
    }
    
    public string GetName() => _name;

    public string GetAuthor() => _author;

    public string GetVersion() => _version;

    public string GetMainClass() => _mainClass;
}