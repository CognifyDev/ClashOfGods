using COG.Config.Impl;
using COG.UI.CustomOption;
using System.Linq;
using System;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch(typeof(GameOptionsMenu))]
internal static class GameOptionMenuPatch
{
    [HarmonyPatch(nameof(GameOptionsMenu.CreateSettings))]
    [HarmonyPostfix]
    public static void OnSettingsCreated(GameOptionsMenu __instance)
    {
        int layer = GameOptionsMenu.MASK_LAYER;
        var xStart = GameOptionsMenu.START_POS_X;
        var headerX = GameOptionsMenu.HEADER_X;
        var headerOffset = GameOptionsMenu.HEADER_SCALE;

        var settingMap = CustomOption.Options.Where(o => o is { Page: CustomOption.TabType.General }).ToDictionary(o => o!, o => o!.ToVanillaOptionData());

        float num = -(__instance.scrollBar.ContentYBounds.max + 1.65f);
        var header = Object.Instantiate(__instance.categoryHeaderOrigin, __instance.settingsContainer);
        header.SetHeader(StringNames.None, layer);
        header.Title.text = LanguageConfig.Instance.GeneralHeaderTitle;
        header.transform.localScale = Vector3.one * headerOffset;
        header.transform.localPosition = new Vector3(headerX, num, -2f);
        num -= headerOffset;

        foreach (var (option, vanillaSetting) in settingMap)
        {
            OptionBehaviour? behaviour = null;
            if (vanillaSetting == null) return;
            switch (vanillaSetting.Type)
            {
                case OptionTypes.Checkbox:
                    {
                        behaviour = Object.Instantiate(__instance.checkboxOrigin, __instance.settingsContainer);
                        break;
                    }
                case OptionTypes.String:
                    {
                        behaviour = Object.Instantiate(__instance.stringOptionOrigin, __instance.settingsContainer);
                        break;
                    }
                case OptionTypes.Float:
                case OptionTypes.Int:
                    {
                        behaviour = Object.Instantiate(__instance.numberOptionOrigin, __instance.settingsContainer);
                        break;
                    }
            }

            if (!behaviour) continue;

            behaviour!.transform.localPosition = new Vector3(xStart, num, -2f);
            behaviour.SetClickMask(__instance.ButtonClickMask);
            behaviour.SetUpFromData(vanillaSetting, layer);
            __instance.Children.Add(behaviour);
            option.OptionBehaviour = behaviour;

            num -= 0.45f;
        }

        __instance.scrollBar.SetYBoundsMax(-num - 1.65f);
    }

    [HarmonyPatch(nameof(GameOptionsMenu.ValueChanged))]
    [HarmonyPrefix]
    public static bool OnValueChangedVanilla(OptionBehaviour __instance)
    {
        return !CustomOption.TryGetOption(__instance, out _);
    } 
}