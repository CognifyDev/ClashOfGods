using System.Linq;
using COG.Listener;

namespace COG.Patch;

[HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
internal class DeadBodyClickPatch
{
    public static bool Prefix(DeadBody __instance)
    {
        var returnAble = true;
        foreach (var unused in ListenerManager.GetManager().GetListeners().Where(listener => !listener.OnDeadBodyClick(__instance)))
            returnAble = false;

        return returnAble;
    }
}