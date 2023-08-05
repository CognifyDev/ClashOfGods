using COG.Listener;
using COG.UI.ModOption;

namespace COG.Patch;

[HarmonyPatch(typeof(OptionsMenuBehaviour))]
internal class ModOptionsPatch
{
    [HarmonyPatch(nameof(OptionsMenuBehaviour.Start))]
    [HarmonyPostfix]
    public static void OptionMenuBehaviour_StartPostfix(OptionsMenuBehaviour __instance)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.OnSettingInit(__instance);
    }

    [HarmonyPatch(nameof(OptionsMenuBehaviour.Close))]
    [HarmonyPostfix]
    public static void OptionMenuBehaviour_ClosePostfix()
    {
        foreach (var btn in ModOption.Buttons)
            if (btn.ToggleButton != null)
                btn.ToggleButton.gameObject.SetActive(false);
    }
}

[HarmonyPatch(typeof(TabGroup), nameof(TabGroup.Open))]
internal class OnTabGroupOpen
{
    private static void Postfix(TabGroup __instance)
    {
        foreach (var btn in ModOption.Buttons)
            if (btn.ToggleButton != null)
                btn.ToggleButton.gameObject.SetActive(false);
    }
}