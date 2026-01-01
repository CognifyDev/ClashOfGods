using System.Linq;
using COG.Config.Impl;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch(typeof(GameOptionsMenu))]
internal static class GameOptionMenuPatch
{
    [HarmonyPatch(nameof(GameOptionsMenu.CreateSettings))]
    [HarmonyPostfix]
    public static void OnSettingsCreation(GameOptionsMenu __instance)
    {
        var layer = GameOptionsMenu.MASK_LAYER;
        var xStart = GameOptionsMenu.START_POS_X;
        var headerX = GameOptionsMenu.HEADER_X;
        var headerOffset = GameOptionsMenu.HEADER_SCALE;
        var space = GameOptionsMenu.SPACING_Y;
        var mapPickerHeight = GameOptionsMenu.MAP_PICKER_HEIGHT;

        var settingMap = CustomOption.Options.Where(o => o is { Page: CustomOption.TabType.General })
            .ToDictionary(o => o, o => o.ToVanillaOptionData());

        var num = -(__instance.scrollBar.ContentYBounds.max + mapPickerHeight);
        var header = Object.Instantiate(__instance.categoryHeaderOrigin, __instance.settingsContainer);
        header.SetHeader(StringNames.None, layer);
        header.Title.text = LanguageConfig.Instance.GeneralHeaderTitle;
        var transform = header.transform;
        transform.localScale = Vector3.one * headerOffset;
        transform.localPosition = new Vector3(headerX, num, -2f);
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

            num -= space;
        }

        __instance.scrollBar.SetYBoundsMax(-num - mapPickerHeight);
    }

    [HarmonyPatch(nameof(GameOptionsMenu.Update))]
    [HarmonyPostfix]
    public static void OnMenuUpdate(GameOptionsMenu __instance)
    {
        var options = CustomOption.Options
            .Where(o => o is { OptionBehaviour: not null, Page: CustomOption.TabType.General }).ToList();
        var num = GameOptionsMenu.START_POS_Y;
        var headerHeight = GameOptionsMenu.HEADER_HEIGHT;
        var space = GameOptionsMenu.SPACING_Y;
        var xStart = GameOptionsMenu.START_POS_X;
        var mapPickerHeight = GameOptionsMenu.MAP_PICKER_HEIGHT;

        foreach (var category in GameManager.Instance.GameSettingsList.AllCategories) // Calculate vanilla option
        {
            num -= headerHeight;
            foreach (var _ in category.AllGameSettings)
                num -= space;
        }

        num -= headerHeight;
        options.ForEach(o =>
        {
            var x = xStart;

            if (o.Parent is { ValueRule: BoolOptionValueRule })
            {
                var active = o.Parent.GetBool();
                o.OptionBehaviour!.gameObject.SetActive(active);
                o.OptionBehaviour!.LabelBackground.enabled = false;
                x += 0.5f;
                if (!active) return;
            }

            o.OptionBehaviour!.gameObject.SetActive(true);
            o.OptionBehaviour!.transform.localPosition = new Vector3(x, num, -2f);

            num -= space;
        });

        var scroller = __instance.scrollBar;
        scroller.SetYBoundsMax(-num - mapPickerHeight);
        scroller.UpdateScrollBars();
        if (Mathf.Approximately(scroller.GetScrollPercY(), 1)) scroller.ScrollPercentY(1); // 修复可能的超出滚动条范围的bug
    }

    [HarmonyPatch(nameof(GameOptionsMenu.ValueChanged))]
    [HarmonyPrefix]
    public static bool OnVanillaValueChange(OptionBehaviour option)
    {
        return !CustomOption.TryGetOption(option, out _);
    }
}