using COG.Listener;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Impl;
using COG.UI.ModOption;
using COG.Utils;
using System.Linq;

namespace COG.Patch;

[HarmonyPatch(typeof(OptionsMenuBehaviour))]
internal class ModOptionsPatch
{
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Start))]
    [HarmonyPostfix]
    public static void OptionMenuBehaviourStartPostfix(OptionsMenuBehaviour __instance)
    {
        ListenerManager.GetManager()
            .ExecuteHandlers(new OptionsMenuBehaviourStartEvent(__instance), EventHandlerType.Postfix);
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
        ModOption.Buttons.Where(b => b.ToggleButton).Select(b => b.ToggleButton!.gameObject).
            Concat(ModOptionListener.HotkeyButtons).Do(o => o.Destroy());
        ModOptionListener.ResetHotkeyState();
    }
}

[HarmonyPatch(typeof(TabGroup), nameof(TabGroup.Open))]
internal class OnTabGroupOpen
{
    private static void Postfix()
    {
        ModOption.Buttons.Where(b => b.ToggleButton).Select(b => b.ToggleButton!.gameObject).
            Concat(ModOptionListener.HotkeyButtons).Do(o => o.Destroy());
        ModOptionListener.ResetHotkeyState();
    }
}