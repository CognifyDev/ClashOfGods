using COG.Listener;
using COG.Utils;
using System.Linq;

namespace COG.Patch;

[HarmonyPatch(typeof(HudManager))]
internal class HudManagerPatch
{
    [HarmonyPatch(nameof(HudManager.Start))]
    [HarmonyPostfix]
    public static void OnHudStart(HudManager? __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners().ToList())
            listener.OnHudStart(__instance);
    }

    [HarmonyPatch(nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void OnHudUpdate(HudManager __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners().ToList())
            listener.OnHudUpdate(__instance);
    }

    [HarmonyPatch(nameof(HudManager.OnDestroy))]
    [HarmonyPostfix]
    public static void OnHudDestroy(HudManager __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners().ToList())
            listener.OnHudDestroy(__instance);
    }
}