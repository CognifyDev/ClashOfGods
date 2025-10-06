using System;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace COG.Patch;

[HarmonyPatch(typeof(GamePresetsTab))]
public static class PresetsButtonsPatch
{
    [HarmonyPatch(nameof(GamePresetsTab.Start))]
    [HarmonyPostfix]
    public static void PresetButtonHook(GamePresetsTab __instance)
    {
        var std = __instance.StandardPresetButton;
        var alter = __instance.SecondPresetButton;

        // Destroy selectable background
        DestroySelectableSprite(std.gameObject);
        DestroySelectableSprite(alter.gameObject);

        var transform = std.transform;
        transform.localPosition = new Vector3(-1.5f, 0.2f, 0);
        var transform1 = alter.transform;
        transform1.localPosition = new Vector3(2.1f, 0.2f, 0);
        transform.localScale = transform1.localScale = new Vector3(1.1f, 1.1f, 1);

        void DestroySelectableSprite(GameObject go)
        {
            var trans = go.transform;
            trans.transform.FindChild("Active").FindChild("SelectionBackground").gameObject.TryDestroy();
            trans.transform.FindChild("Selected").FindChild("SelectionBackground").gameObject.TryDestroy();
        }

        // Set button text
        __instance.StandardRulesText.text = LanguageConfig.Instance.LoadPreset;
        __instance.AlternateRulesText.text = LanguageConfig.Instance.SavePreset;

        // Set button OnClick action
        std.OnClick = new Button.ButtonClickedEvent();
        std.OnClick.AddListener((UnityAction)new Action(() =>
        {
            ResetActiveState(std);
            CustomOption.OnLoadPresetButtonClicked();
        }));

        alter.OnClick = new Button.ButtonClickedEvent();
        alter.OnClick.AddListener((UnityAction)new Action(() =>
        {
            ResetActiveState(alter);
            CustomOption.OnSavePresetButtonClicked();
        }));

        __instance.PresetDescriptionText.gameObject.SetActive(false); // Hide preset introduction text
        __instance.transform.Find("DividerImage").gameObject.TryDestroy();

        void ResetActiveState(PassiveButton button)
        {
            button.SelectButton(false);
            __instance.ClickPresetButton(RulesPresets.Custom, false);
            __instance.StandardPresetButton.SelectButton(false);
            __instance.SecondPresetButton.SelectButton(false);
        }
    }

    [HarmonyPatch(nameof(GamePresetsTab.ClickPresetButton))]
    [HarmonyPrefix]
    public static void OnButtonClickAlwaysCustomPreset(ref RulesPresets preset)
    {
        preset = RulesPresets.Custom;
    }
}