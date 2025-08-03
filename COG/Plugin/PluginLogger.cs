namespace COG.Plugin;

public class PluginLogger
{
    private readonly PluginDescription _description;
    
    public PluginLogger(PluginDescription description)
    {
        _description = description;
    }

    private string GetFullString(object? msg)
    {
        return $"[{_description.Name}] {msg}";
    }

#pragma warning disable IDE1006 // Use lower case to match JS name style
    public void debug(object? msg)
    {
        Main.Logger.NativeLogger.LogDebug(GetFullString(msg));
    }

    public void info(object? msg)
    {
        Main.Logger.NativeLogger.LogInfo(GetFullString(msg));
    }

    public void warn(object? msg)
    {
        Main.Logger.NativeLogger.LogWarning(GetFullString(msg));
    }

    public void fatal(object? msg)
    {
        Main.Logger.NativeLogger.LogFatal(GetFullString(msg));
    }

    public void error(object? msg)
    {
        Main.Logger.NativeLogger.LogError(GetFullString(msg));
    }

    public void message(object? msg)
    {
        Main.Logger.NativeLogger.LogMessage(GetFullString(msg));
    }
#pragma warning restore IDE1006
}