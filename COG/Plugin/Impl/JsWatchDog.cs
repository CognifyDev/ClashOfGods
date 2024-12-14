using COG.Config.Impl;
using COG.Utils;

namespace COG.Plugin.Impl;

public class JsWatchDog : WatchDog
{
    public override void Check()
    {
        var usage = SystemUtils.GetCPUUsage();

        var maxCPUUsage = SettingsConfig.Instance.MaxCPUUsage;
        if (maxCPUUsage > 0 && usage > maxCPUUsage)
        {
            throw new System.Exception("your plugins have been over max cpu usage");
        }
    }

    public override void Reset()
    {
    }
}