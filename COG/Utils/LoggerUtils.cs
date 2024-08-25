using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;

namespace COG.Utils;

public class StackTraceLogger
{
    public StackTraceLogger(string name)
    {
        BepInExLogger = BepInEx.Logging.Logger.CreateLogSource(name);
        RegisteredCustomLogger.Add(this);
    }

    public static List<StackTraceLogger> RegisteredCustomLogger { get; } = new();
    public List<MethodInfo> DisabledMethodSource { get; } = new();
    private ManualLogSource BepInExLogger { get; }

    public void DisableSource(Type toDisable)
    {
        DisabledMethodSource.AddRange(toDisable.GetMethods());
    }

    public void DisableMethod(MethodInfo toDisable)
    {
        DisabledMethodSource.Add(toDisable);
    }

    public void EnableSource(Type toEnable)
    {
        DisabledMethodSource.RemoveAll(m => toEnable.GetMethods().Contains(m));
    }

    public void EnableMethod(MethodInfo toEnable)
    {
        DisabledMethodSource.Remove(toEnable);
    }

    public void LogDebug(object? msg, string? customName = null)
    {
        BepInExLogger.LogDebug(GetFullString(msg, customName));
    }

    public void LogInfo(object? msg, string? customName = null)
    {
        BepInExLogger.LogInfo(GetFullString(msg, customName));
    }

    public void LogWarning(object? msg, string? customName = null)
    {
        BepInExLogger.LogWarning(GetFullString(msg, customName));
    }

    public void LogFatal(object? msg, string? customName = null)
    {
        BepInExLogger.LogFatal(GetFullString(msg, customName));
    }

    public void LogError(object? msg, string? customName = null)
    {
        BepInExLogger.LogError(GetFullString(msg, customName));
    }

    public void LogMessage(object? msg, string? customName = null)
    {
        BepInExLogger.LogMessage(GetFullString(msg, customName));
    }

    private string GetFullString(object? data, string? customName = null)
    {
        var st = new StackTrace().GetFrame(2);
        var method = st?.GetMethod();
        data ??= "";

        if (customName != null) return $"[{customName}] {data}";
        if (st == null || method == null) return $"[Unknown] {data}";
        if (DisabledMethodSource.Contains(method)) return "";
        var type = method.DeclaringType;

        return $"[{type?.FullName}::{method.Name}] {data}";
    }
}

public static class LoggerUtils
{
    public static T Dump<T>(this T obj)
    {
        Main.Logger.LogInfo(obj?.ToString() ?? "(null)", "Dump");
        return obj;
    }

    public static IEnumerable<T> Dump<T>(this IEnumerable<T> obj, Func<T, string> selector)
    {
        Main.Logger.LogInfo(obj.Select(selector).AsString(), "Dump");
        return obj;
    }
}