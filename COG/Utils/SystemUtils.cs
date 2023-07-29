using Il2CppSystem;
using Microsoft.Win32;

namespace COG.Utils;

public class SystemUtils
{
    /// <summary>
    /// 获取时间戳
    /// </summary>
    /// <param name="isMillisecond">是否毫秒</param>
    /// <returns>当前时间戳</returns>
    public static long GetTimeStamp(bool isMillisecond = false)
    {
        var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        var timeStamp = isMillisecond ? Convert.ToInt64(ts.TotalMilliseconds) : Convert.ToInt64(ts.TotalSeconds); 
        return timeStamp; 
    }

    public static int GetLanguageAsLcid() => System.Globalization.CultureInfo.CurrentCulture.LCID;
    
    public static string GetHwid()
    {
        #pragma warning disable CA1416
        return System.Security.Principal.WindowsIdentity.GetCurrent().User!.Value.GetSHA1Hash();
    }

    private static string GetRegistryValue(string keyPath, string valueName)
    {
        string value = string.Empty;
        
        try
        {
            using (RegistryKey? key = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    value = key.GetValue(valueName)?.ToString()?.Trim()!;
                }
            }
        }
        catch (System.Exception)
        {
            value = string.Empty;
        }
        
        return value;
    }
}