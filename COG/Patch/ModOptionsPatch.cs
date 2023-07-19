using COG.Listener;
using COG.Listener.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COG.Patch
{
    [HarmonyPatch(typeof(OptionsMenuBehaviour))]
    class ModOptionsPatch
    {
        [HarmonyPatch(nameof(OptionsMenuBehaviour.Start))]
        [HarmonyPostfix]
        public static void OptionMenuBehaviour_StartPostfix(OptionsMenuBehaviour __instance)
        {
            foreach (var listener in ListenerManager.GetManager().GetListeners())
            {
                listener.OnSettingInit(__instance);
            }
        }
        [HarmonyPatch(nameof(OptionsMenuBehaviour.Close))]
        [HarmonyPostfix]
        public static void OptionMenuBehaviour_ClosePostfix()
        {
            foreach (var btn in ModOption.buttons) btn.ToggleButton.gameObject.SetActive(false);
        }
    }
    [HarmonyPatch(typeof(TabGroup),nameof(TabGroup.Open))]
    class OnTabGroupOpen
    {
        static void Postfix(TabGroup __instance)
        {
            if (__instance.name == "GeneralButton") return;
            foreach (var btn in ModOption.buttons) btn.ToggleButton.gameObject.SetActive(false);
        }
    }
}
