using COG.Listener;

namespace COG.Patch;

[HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
internal class DeadBodyClickPatch
{
    public static bool Prefix(DeadBody __instance)
    {
        var returnAble = true;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
            if (!listener.OnDeadBodyClick(__instance))
                returnAble = false;

        return returnAble;
    }
}