using System;
using System.IO;
using System.Net;
using System.Text;
using COG.Exception.Plugin;
using COG.Plugin.Loader.Controller.ClassType.Classes.Globe;
using COG.Plugin.Loader.Controller.Function;
using COG.Utils;
using NLua;

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
    private Lua LuaController { get; }

    private LuaFunction OnEnableFunction { get; }
    private LuaFunction OnDisableFunction { get; }

    public LuaPluginLoader(string scriptPath)
    {
        ScriptPath = scriptPath;
        var directoryInfo = new DirectoryInfo(ScriptPath);

        if (!CheckDirectory(directoryInfo))
            throw new CannotLoadPluginException($"{directoryInfo.Name} not a legal plugin");
        LuaController = new Lua();
        LuaController.State.Encoding = Encoding.UTF8;

        var pluginYaml = Yaml.LoadFromFile(ScriptPath + "\\plugin.yml");
        try
        {
            _name = pluginYaml!.GetString("name")!;
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

    private static bool CheckDirectory(FileSystemInfo directoryInfo)
    {
        return directoryInfo.Exists && File.Exists(directoryInfo.FullName + "\\plugin.yml");
    }

    private bool CheckPlugin()
    {
        var onEnableFunction = LuaController.GetFunction("onEnable");
        var onDisableFunction = LuaController.GetFunction("onDisable");
        return onEnableFunction != null && onDisableFunction != null;
    }

    public void MakeLanguage()
    {
        LuaController.LoadCLRPackage();

        // register global value
        LuaController["COG_VERSION"] = Main.PluginVersion;
        LuaController["COG_NAME"] = Main.PluginName;
        LuaController["COG_DISPLAY_NAME"] = Main.DisplayName;
        LuaController["COG_PLUGIN_GUID"] = Main.PluginGuid;

        LuaController["IS_BETA_VERSION"] = Main.VersionInfo.Beta;

        LuaController["controller"] = new PluginController(LuaController, this);
#pragma warning disable SYSLIB0014
        LuaController["web"] = new WebClient();
#pragma warning restore SYSLIB0014

        // register methods
        var functionsType = typeof(Functions);
        foreach (var methodInfo in functionsType.GetMethods())
        {
            var attributes = methodInfo.GetCustomAttributes(typeof(FunctionRegisterAttribute), false);
            if (attributes.Length != 1) continue;

            if (attributes[0] is FunctionRegisterAttribute functionRegisterAttribute)
                LuaController.RegisterFunction(functionRegisterAttribute.FunctionName, null, methodInfo);
        }
        /*
        // register class types
        var assembly = Assembly.Load("ClashOfGods");
        var types = assembly.GetTypes().Where(
            type => type.Namespace != null
                    && type.Namespace.ToLower().StartsWith("COG.Plugin.Loader.Controller.ClassType")
                    && type.GetCustomAttributes(typeof(ClassRegisterAttribute), false).Length > 0
                    && type.IsClass
            )
            .ToArray();
        Main.Logger.LogInfo($"{types.Length} were found to register");
        foreach (var type in types)
        {
            var attributes = type.GetCustomAttributes(typeof(ClassRegisterAttribute), false);
            if (attributes[0] is ClassRegisterAttribute classRegisterAttribute)
            {
                LuaController[classRegisterAttribute.ClassName] = ;
            }
        }
        */
    }

    public void OnEnable()
    {
        OnEnableFunction.Call();
    }

    public void OnDisable()
    {
        OnDisableFunction.Call();
    }

    public string GetName()
    {
        return _name;
    }

    public string GetAuthor()
    {
        return _author;
    }

    public string GetVersion()
    {
        return _version;
    }

    public string GetMainClass()
    {
        return _mainClass;
    }
}