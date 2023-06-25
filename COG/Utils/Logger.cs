using System.Runtime.CompilerServices;

namespace COG.Utils;

using System;
using LogLevel = BepInEx.Logging.LogLevel;

public class Logger
{
    private static void SendToFile(string text, LogLevel level = LogLevel.Info, string tag = "", bool escapeCRLF = true, int lineNumber = 0, string fileName = "")
    {
        var logger = Main.Logger;
        string t = DateTime.Now.ToString("HH:mm:ss");
        if (escapeCRLF)
            text = text.Replace("\r", "\\r").Replace("\n", "\\n");
        string logText = $"[{t}][{tag}]{text}";
        switch (level)
        {
            case LogLevel.Info:
                logger.LogInfo(logText);
                break;
            case LogLevel.Warning:
                logger.LogWarning(logText);
                break;
            case LogLevel.Error:
                logger.LogError(logText);
                break;
            case LogLevel.Fatal:
                logger.LogFatal(logText);
                break;
            case LogLevel.Message:
                logger.LogMessage(logText);
                break;
            default:
                logger.LogWarning("Error:Invalid LogLevel");
                logger.LogInfo(logText);
                break;
        }
    }
    
    public static void Info(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(text, LogLevel.Info, tag, escapeCRLF, lineNumber, fileName);
    public static void Warn(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(text, LogLevel.Warning, tag, escapeCRLF, lineNumber, fileName);
    public static void Error(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(text, LogLevel.Error, tag, escapeCRLF, lineNumber, fileName);
    public static void Fatal(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(text, LogLevel.Fatal, tag, escapeCRLF, lineNumber, fileName);
    public static void Msg(string text, string tag, bool escapeCRLF = true, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(text, LogLevel.Message, tag, escapeCRLF, lineNumber, fileName);

    public static void Exception(Exception ex, string tag, [CallerLineNumber] int lineNumber = 0, [CallerFilePath] string fileName = "") =>
        SendToFile(ex.ToString(), LogLevel.Error, tag, false, lineNumber, fileName);
}