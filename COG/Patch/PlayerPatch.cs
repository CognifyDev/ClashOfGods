using COG.Listener;
using HarmonyLib;

namespace COG.Patch;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
class PlayerMurderPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        
        bool returnAble = false;
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            if (!listener.OnPlayerMurder(__instance, target) && !returnAble)
            {
                returnAble = true;
            }
        }

        if (returnAble) return false;

        return true;
    }

}