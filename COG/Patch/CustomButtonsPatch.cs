using COG.Listener;
using COG.Listener.Event.Impl.HManager;
using COG.NewListener;

namespace COG.Patch;

[HarmonyPatch(typeof(HudManager))]
internal class HudManagerPatch
{
    [HarmonyPatch(nameof(HudManager.Start))]
    [HarmonyPostfix]
    public static void OnHudStart(HudManager __instance)
    {
        ListenerManager.GetManager().ExecuteHandlers(new HudManagerStartEvent(__instance), EventHandlerType.Postfix);
    }

    [HarmonyPatch(nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void OnHudUpdate(HudManager __instance)
    {
        ListenerManager.GetManager().ExecuteHandlers(new HudManagerUpdateEvent(__instance), EventHandlerType.Postfix);
    }

    [HarmonyPatch(nameof(HudManager.OnDestroy))]
    [HarmonyPostfix]
    public static void OnHudDestroy(HudManager __instance)
    {
        ListenerManager.GetManager().ExecuteHandlers(new HudManagerDestroyEvent(__instance), EventHandlerType.Postfix);
    }
}