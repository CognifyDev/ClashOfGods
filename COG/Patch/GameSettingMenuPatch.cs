using COG.Config.Impl;

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
}