using COG.Config.Impl;
using COG.Role;
using COG.UI.CustomOption;
using COG.Utils;
using System;
using System.Linq;
using COG.UI.CustomOption.ValueRules.Impl;

namespace COG.Patch;

[HarmonyPatch(typeof(GameSettingMenu))]
internal static class GameSettingMenuPatch
{
    [HarmonyPatch(nameof(GameSettingMenu.Close))]
    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    [HarmonyPostfix]
    public static void ResetOptionData()
    {
        RoleOptionPatch.CampTabs.Clear();
        RoleOptionPatch.CurrentTab = null;
        RoleOptionPatch.CurrentAdvancedTabFor = null;
        RoleOptionPatch.ScrollerLocationPercent = 0f;

        ControllerManager.Instance.CurrentUiState.MenuName = ""; // Fix player is unmoveable after closing menu
    }

    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    [HarmonyPostfix]
    public static void OnMenuInitialization(GameSettingMenu __instance)
    {
        if (AmongUsClient.Instance.AmHost) return;
        __instance.GamePresetsButton.gameObject.SetActive(false);
        var pos1 = __instance.GamePresetsButton.transform.localPosition;
        var pos2 = __instance.GameSettingsButton.transform.localPosition;
        __instance.GameSettingsButton.transform.localPosition = pos1;
        __instance.RoleSettingsButton.transform.localPosition = pos2;
    }

    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab))]
    [HarmonyPrefix]
    public static void OnTabChanged(ref int tabNum, bool previewOnly)
    {
        if (previewOnly || AmongUsClient.Instance.AmHost) return;
        if (tabNum == 0) tabNum = 1;
    }

    [HarmonyPatch(nameof(GameSettingMenu.Update))]
    [HarmonyPostfix]
    public static void OnMenuUpdate(GameSettingMenu __instance)
    {
        if (AmongUsClient.Instance.AmHost) return;
        var tabNum = __instance.GameSettingsTab.isActiveAndEnabled ? 1 : 2;
        var handler = LanguageConfig.Instance.GetHandler("game-setting.view-description");
        __instance.MenuDescriptionText.text = tabNum switch
        {
            1 => handler.GetString("general"),
            2 => handler.GetString("role"),
            _ => ""
        };
    }

    // DANGER: refresh methods can not be executed.

    [HarmonyPatch(typeof(RolesSettingsMenu), nameof(RolesSettingsMenu.RefreshChildren))]
    [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.RefreshChildren))]
    [HarmonyPostfix]
    private static void OnRefreshChildren()
    {
        // This patch is for clients who are previewing the settings
        // and hosts who has loaded the preset to update options when the host has changed settings

        foreach (var role in CustomRoleManager.GetManager().GetRoles()
                     .Where(r => r is { IsBaseRole: false, ShowInOptions: true }))
        {
            var roleRoleNumberOption = role.RoleNumberOption;
            if (roleRoleNumberOption == null)
            {
                continue;
            }
            roleRoleNumberOption.OptionBehaviour?.Cast<RoleOptionSetting>().UpdateValuesAndText(null);
            role.RoleOptions.Where(o => o.OptionBehaviour).ForEach(o =>
            {
                var behaviour = o.OptionBehaviour!;
                NumberOption numberOption;
                StringOption stringOption;
                ToggleOption toggleOption;
                if ((numberOption = behaviour.GetComponent<NumberOption>()) != null)
                    numberOption.Value = o.GetFloat();
                else if ((stringOption = behaviour.GetComponent<StringOption>()) != null)
                    stringOption.Value = o.Selection;
                else if ((toggleOption = behaviour.GetComponent<ToggleOption>()) != null)
                    toggleOption.CheckMark.enabled = o.GetBool();
            });
        }

        if (!(GameSettingMenu.Instance && GameSettingMenu.Instance.GameSettingsTab))
            return;

        GameSettingMenu.Instance.GameSettingsTab.Children?.ForEach(new Action<OptionBehaviour>(o =>
        {
            if (CustomOption.TryGetOption(o, out _)) return;
            NumberOption numberOption;
            StringOption stringOption;
            ToggleOption toggleOption;
            if ((numberOption = o.GetComponent<NumberOption>()) != null)
                numberOption.oldValue = int.MinValue;
            else if ((stringOption = o.GetComponent<StringOption>()) != null)
                stringOption.oldValue = -1;
            else if ((toggleOption = o.GetComponent<ToggleOption>()) != null)
                toggleOption.oldValue = !toggleOption.CheckMark.enabled;
        }));
    }
}