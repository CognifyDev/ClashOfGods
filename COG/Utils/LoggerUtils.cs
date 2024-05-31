using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;

namespace COG.Utils;

public class StackTraceLogger
{
    public static List<StackTraceLogger> RegisteredCustomLogger { get; } = new();
    private ManualLogSource BepInExLogger { get; }
    public StackTraceLogger(string name)
    {
        BepInExLogger = BepInEx.Logging.Logger.CreateLogSource(name);
        RegisteredCustomLogger.Add(this);
    }

    public void LogDebug(object? msg)
    {
        if (msg == null) return;
        BepInExLogger.LogDebug(GetFullString(msg));
    }

    public void LogInfo(object? msg)
    {
        if (msg == null) return;
        BepInExLogger.LogInfo(GetFullString(msg));
    }

    public void LogWarning(object? msg)
    {
        if (msg == null) return;
        BepInExLogger.LogWarning(GetFullString(msg));
    }

    public void LogFatal(object? msg)
    {
        if (msg == null) return;
        BepInExLogger.LogFatal(GetFullString(msg));
    }

    public void LogError(object? msg)
    {
        if (msg == null) return;
        BepInExLogger.LogError(GetFullString(msg));
    }

    public void LogMessage(object? msg)
    {
        if (msg == null) return;
        BepInExLogger.LogMessage(GetFullString(msg));
    }

    private string GetFullString(object data)
    {
        var st = new StackTrace().GetFrame(2);
        var method = st?.GetMethod();

        if (st == null || method == null) return "[Unknown]";
        var type = method.DeclaringType;

        return $"[{type?.FullName}::{method.Name}] {data}";
    }
}