using Il2CppSystem;

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
}