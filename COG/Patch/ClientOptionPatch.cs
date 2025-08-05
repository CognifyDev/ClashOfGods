using COG.Listener;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Impl;
using COG.UI.ClientOption;
using COG.Utils;
using System.Linq;

namespace COG.Patch;

[HarmonyPatch(typeof(OptionsMenuBehaviour))]
internal class ClientOptionPatch
{
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Start))]
    [HarmonyPostfix]
    public static void OptionMenuBehaviourStartPostfix(OptionsMenuBehaviour __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new OptionsMenuBehaviourStartEvent(__instance), EventHandlerType.Postfix);
    }

    [HarmonyPatch(nameof(OptionsMenuBehaviour.Start))]
    [HarmonyPrefix]
    public static void OptionMenuBehaviourStartPrefix(OptionsMenuBehaviour __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new OptionsMenuBehaviourStartEvent(__instance), EventHandlerType.Prefix);
    }

    [HarmonyPatch(nameof(OptionsMenuBehaviour.Update))]
    [HarmonyPostfix]
    public static void OptionMenuBehaviourUpdatePostfix(OptionsMenuBehaviour __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new OptionsMenuBehaviourUpdateEvent(__instance), EventHandlerType.Postfix);
    }

    [HarmonyPatch(nameof(OptionsMenuBehaviour.Close))]
    [HarmonyPostfix]
    public static void OptionMenuBehaviourClosePostfix()
    {
        ClientOptionListener.ModTabContainer!.SetActive(false);
        ClientOptionListener.ResetHotkeyState();
    }

    [HarmonyPatch(nameof(OptionsMenuBehaviour.OpenTabGroup))]
    [HarmonyPrefix]
    public static void OptionMenuBehaviourOpenTabGroupPrefix(OptionsMenuBehaviour __instance, int index)
    {
        try
        {
            ClientOptionListener.ModTabContainer!.SetActive(__instance.Tabs[index].name == ClientOptionListener.ModTabName);
        }
        catch
        {
            ClientOptionListener.ModTabContainer!.SetActive(false);
        }

    }
}

[HarmonyPatch(typeof(TabGroup), nameof(TabGroup.Open))]
internal class OnTabGroupOpen
{
    private static void Postfix()
    {
        ClientOptionListener.ResetHotkeyState();
        ClientOptionListener.HotkeyButtons.DoIf(o => o, o => o.SetActive(false));
    }
}

[HarmonyPatch(typeof(TransitionOpen._AnimateClose_d__7), nameof(TransitionOpen._AnimateClose_d__7.MoveNext))]
internal static class OnAnimateClose
{
    private static void Postfix(TransitionOpen._AnimateClose_d__7 __instance)
    {
        if (__instance.__4__this.name is "OptionsMenu" or "Menu" && ClientOptionListener.ModTabContainer)
        {
            ClientOptionListener.ModTabContainer!.SetActive(false);
        }
    }
}