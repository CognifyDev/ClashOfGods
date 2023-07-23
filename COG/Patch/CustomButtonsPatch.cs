using COG.Listener;
using System;
using System.Collections.Generic;

namespace COG.Patch
{
    [HarmonyPatch(typeof(HudManager))]
    class HudManagerPatch
    {
        [HarmonyPatch(nameof(HudManager.Start))]
        [HarmonyPostfix]
        public static void OnHudStart(HudManager __instance)
        {
            foreach(var listener in ListenerManager.GetManager().GetListeners())
            {
                listener.OnHudStart(__instance);
            }
        }

        [HarmonyPatch(nameof(HudManager.Update))]
        [HarmonyPostfix]
        public static void OnHudUpdate(HudManager __instance)
        {
            foreach (var listener in ListenerManager.GetManager().GetListeners())
            {
                listener.OnHudUpdate();
            }
        }
    }
}
