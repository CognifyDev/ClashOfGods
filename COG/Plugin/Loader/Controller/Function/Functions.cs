using System.IO;
using System.Linq;
using COG.Plugin.Manager;

namespace COG.Plugin.Loader.Controller.Function;

public class Functions
{
    [FunctionRegister("writeFileByBytes")]
    public static void WriteFileByBytes(string path, string bytes)
    {
        File.WriteAllBytes(path, bytes.Split(",").Select(byte.Parse).ToArray());
    }

    [FunctionRegister("getFileNames")]
    public static string[] GetFileNames()
    {
        return Directory.GetFiles("./");
    }

    [FunctionRegister("readFileAsBytes")]
    public static byte[] ReadFileAsBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    [FunctionRegister("logInfo")]
    public static void Info(string param)
    {
        Main.Logger.LogInfo(param);
    }

    [FunctionRegister("info")]
    public static void Info0(string param)
    {
        Info(param);
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
            return plugin.GetAuthor();

        return "null";
    }

    [FunctionRegister("getVersion")]
    public static string GetVersion(string pluginName)
    {
        foreach (var plugin in PluginManager.GetPlugins().Where(plugin => plugin.GetName().Equals(pluginName)))
            return plugin.GetVersion();

        return "null";
    }

    [FunctionRegister("getMainClass")]
    public static string GetMainClass(string pluginName)
    {
        foreach (var plugin in PluginManager.GetPlugins().Where(plugin => plugin.GetName().Equals(pluginName)))
            return plugin.GetMainClass();

        return "null";
    }
}