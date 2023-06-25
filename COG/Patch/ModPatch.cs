using COG.Utils;
using HarmonyLib;

namespace COG.Patch;

[HarmonyPatch(typeof(ModManager), nameof(ModManager.LateUpdate))]
internal class ModManagerLateUpdatePatch
{
    public static void Prefix(ModManager __instance)
    {
        __instance.ShowModStamp();

        foreach (var task in TaskManager.GetTasks())
        {
            var timestamp = SystemUtils.GetTimeStamp();
            if (task is LateTask lateTask)
            {
                if (lateTask.Time < timestamp)
                {
                    task.Action();
                }
            }
        }
    }
}