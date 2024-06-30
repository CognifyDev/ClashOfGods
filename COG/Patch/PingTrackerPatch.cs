using COG.Listener;
using COG.Listener.Event.Impl.Game;

namespace COG.Patch;

[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
public static class PingTrackerPatch
{
    private static void Postfix(PingTracker __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new PingTrackerUpdateEvent(__instance), EventHandlerType.Postfix);
    }
}
