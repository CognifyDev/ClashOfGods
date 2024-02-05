using System;
using System.IO;
using COG.Exception.Plugin;
using LuaFunction = NLua.LuaFunction;

namespace COG.Plugin.Loader;

[Serializable]
public class LuaPluginLoader : IPlugin
{
    private string ScriptPath { get; }
    private NLua.Lua LuaController { get; }
    
    private LuaFunction OnEnableFunction { get; }
    private LuaFunction OnDisableFunction { get; }

    public LuaPluginLoader(string scriptPath)
    {
        ScriptPath = scriptPath;
        var fileInfo = new FileInfo(ScriptPath);
        
        if (!CheckFile(fileInfo))
            throw new CannotLoadPluginException($"{fileInfo.Name} not a legal plugin file");
        LuaController = new NLua.Lua();
        LuaController.DoFile(ScriptPath);
        if (!CheckPlugin()) 
            throw new CannotLoadPluginException($"{fileInfo.Name} not a correct plugin with functions");

        OnEnableFunction = LuaController.GetFunction("onEnable");
        OnDisableFunction = LuaController.GetFunction("onDisable");
        MakeLanguage();
    }

    private static bool CheckFile(FileSystemInfo fileInfo) =>
        fileInfo.Exists && fileInfo.Extension.ToLower().Equals(".lua");

    private bool CheckPlugin()
    {
        var onEnableFunction = LuaController.GetFunction("onEnable");
        var onDisableFunction = LuaController.GetFunction("onDisable");
        return onEnableFunction != null && onDisableFunction != null;
    }

    public void MakeLanguage()
    {
        LuaController.RegisterFunction("info", null, typeof(Functions).GetMethod("Info"));
        LuaController.RegisterFunction("error", null, typeof(Functions).GetMethod("Error"));
        LuaController.RegisterFunction("warning", null, typeof(Functions).GetMethod("Warning"));
        LuaController.RegisterFunction("debug", null, typeof(Functions).GetMethod("Debug"));
    }

    private class Functions
    {
        public static void Info(string param)
        {
            Main.Logger.LogInfo(param);
        }
        
        public static void Error(string param)
        {
            Main.Logger.LogError(param);
        }
        
        public static void Warning(string param)
        {
            Main.Logger.LogWarning(param);
        }
        
        public static void Debug(string param)
        {
            Main.Logger.LogDebug(param);
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
}