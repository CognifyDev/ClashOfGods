using COG.Listener;
using COG.Listener.Event.Impl.HManager;
using COG.UI.CustomButton;
using System.Collections.Generic;
using UnityEngine;

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

//[HarmonyPatch(typeof(GridArrange), nameof(GridArrange.GetChildsActive))]
//internal static class AbilityButtonArrangementPatch
//{
//    private static void Postfix()
//    {
//        GridArrange.currentChildren = new(new List<Transform>(
//            GridArrange.currentChildren.ToArray())
//            .RemoveAll(t => t.name.StartsWith(CustomButton.ModdedFlag))); // Let vanilla only arrange for its buttons
//    }
//}