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
        Main.Logger.BepInExLogger.LogDebug(GetFullString(msg));
    }

    public void LogInfo(object? msg)
    {
        Main.Logger.BepInExLogger.LogInfo(GetFullString(msg));
    }

    public void LogWarning(object? msg)
    {
        Main.Logger.BepInExLogger.LogWarning(GetFullString(msg));
    }

    public void LogFatal(object? msg)
    {
        Main.Logger.BepInExLogger.LogFatal(GetFullString(msg));
    }

    public void LogError(object? msg)
    {
        Main.Logger.BepInExLogger.LogError(GetFullString(msg));
    }

    public void LogMessage(object? msg)
    {
        Main.Logger.BepInExLogger.LogMessage(GetFullString(msg));
    }
}