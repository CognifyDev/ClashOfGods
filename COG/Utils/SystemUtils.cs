using System;
using System.Globalization;
#if WINDOWS
using System.Security.Principal;
using Il2CppSystem.Diagnostics;
using Microsoft.Win32;
#endif

namespace COG.Utils;

public static class SystemUtils
{
    /// <summary>
    ///     获取时间戳
    /// </summary>
    /// <param name="isMillisecond">是否毫秒</param>
    /// <returns>当前时间戳</returns>
    public static long GetTimeStamp(bool isMillisecond = false)
    {
        var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        var timeStamp = isMillisecond ? Convert.ToInt64(ts.TotalMilliseconds) : Convert.ToInt64(ts.TotalSeconds);
        return timeStamp;
    }

    public static int GetLanguageAsLcid()
    {
        return CultureInfo.CurrentCulture.LCID;
    }

#if WINDOWS

    public static double GetCpuUsage()
    {
        var performanceCounter = new PerformanceCounter();
        return performanceCounter.NextValue();
    }

    public static string GetHwid()
    {
#pragma warning disable CA1416
        return WindowsIdentity.GetCurrent().User!.Value.GetSHA1Hash();
    }

    public static string? GetRegistryValue(string keyPath, string valueName)
    {
        var value = string.Empty;

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            if (key != null) value = key.GetValue(valueName)?.ToString()?.Trim()!;
        }
        catch
        {
            value = null;
        }

        return value;
    }

    public static bool SetRegistryValue(string keyPath, string valueName, string value)
    {
        try
        {
            using (var key = Registry.LocalMachine.CreateSubKey(keyPath))
            {
                key.SetValue(valueName, value, RegistryValueKind.String);
            }

            return true;
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError($"Failed to set registry value: {e}");
            return false;
        }
    }
#endif
}