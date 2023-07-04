using COG.Listener;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace COG.Patch
{
    public static class PingTrackerUpdate
    {
        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        private static class PingTrackerPatch
        {
            static void Postfix(PingTracker __instance)
            {
                foreach (var listener in ListenerManager.GetManager().GetListeners())
                {
                    listener.OnPingTrackerUpdate(__instance);
                }
            }
        }
    }
}