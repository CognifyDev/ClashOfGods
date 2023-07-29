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
        var hwid = string.Empty;

        try
        {
            // 获取CPU信息
            var cpuInfo = GetRegistryValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0", "ProcessorNameString");

            // 获取主板序列号
            var boardInfo = GetRegistryValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\BIOS", "BaseBoardSerialNumber");

            // 获取硬盘序列号
            var diskInfo = GetRegistryValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DEVICEMAP\Scsi\Scsi Port 0\Scsi Bus 0\Target Id 0\Logical Unit Id 0", "Identifier");

            // 组合硬件信息得到HWID
            hwid = cpuInfo + diskInfo + boardInfo;
        }
        catch (System.Exception)
        {
            // ignored
        }

        return hwid.GetSHA1Hash();
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