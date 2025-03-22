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

    public void LogDebug(object? msg)
    {
        Main.Logger.NativeLogger.LogDebug(GetFullString(msg));
    }

    public void LogInfo(object? msg)
    {
        Main.Logger.NativeLogger.LogInfo(GetFullString(msg));
    }

    public void LogWarning(object? msg)
    {
        Main.Logger.NativeLogger.LogWarning(GetFullString(msg));
    }

    public void LogFatal(object? msg)
    {
        Main.Logger.NativeLogger.LogFatal(GetFullString(msg));
    }

    public void LogError(object? msg)
    {
        Main.Logger.NativeLogger.LogError(GetFullString(msg));
    }

    public void LogMessage(object? msg)
    {
        Main.Logger.NativeLogger.LogMessage(GetFullString(msg));
    }
}