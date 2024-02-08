using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace COG.Utils;

public class SystemUtils
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

    public static string GetHwid()
    {
#pragma warning disable CA1416
        return WindowsIdentity.GetCurrent().User!.Value.GetSHA1Hash();
    }

    private static string GetRegistryValue(string keyPath, string valueName)
    {
        var value = string.Empty;

        try
        {
            using (var key = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null) value = key.GetValue(valueName)?.ToString()?.Trim()!;
            }
        }
        catch (System.Exception)
        {
            value = string.Empty;
        }

        return value;
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public static void OpenMessageBox(string text, string title)
    {
        MessageBox(IntPtr.Zero, text, title, 0x00000000 | 0x00000040);
    }
}