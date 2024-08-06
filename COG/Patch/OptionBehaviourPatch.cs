using COG.Role;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using System.Linq;
using TMPro;

namespace COG.Patch;

[HarmonyPatch(typeof(RoleOptionSetting))]
public static class RoleOptionSettingPatch
{
    [HarmonyPatch(nameof(RoleOptionSetting.UpdateValuesAndText))]
    [HarmonyPrefix]
    public static bool UpdateValuePatch(RoleOptionSetting __instance)
    {
        var role = CustomRoleManager.GetManager().GetRoles()
            .FirstOrDefault(r => r.AllOptions.Any(o => o.OptionBehaviour == __instance));
        if (role == null) return true;

        var playerCount = role.RoleNumberOption!;
        var chance = role.RoleChanceOption!;

        __instance.roleMaxCount = playerCount.GetInt();
        __instance.roleChance = chance.GetInt();
        __instance.countText.text = __instance.roleMaxCount.ToString();
        __instance.chanceText.text = __instance.roleChance.ToString();
        return false;
    }
}

[HarmonyPatch]
public static class OptionBehaviourPatch
{
    private const string InitializeName = nameof(OptionBehaviour.Initialize);

    [HarmonyPatch(typeof(NumberOption), nameof(NumberOption.UpdateValue))]
    [HarmonyPrefix]
    private static bool NumberOptionValueUpdatePatch(NumberOption __instance)
    {
        if (!CustomOption.TryGetOption(__instance, out var customOption)) return true;
        var rule = customOption!.ValueRule;
        if (rule is FloatOptionValueRule)
            customOption.UpdateSelection(__instance.GetFloat());
        else
            customOption.UpdateSelection(__instance.GetInt());
        return false;
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.UpdateValue))]
    [HarmonyPrefix]
    private static bool StringOptionValueUpdatePatch(StringOption __instance)
    {
        if (!CustomOption.TryGetOption(__instance, out var customOption)) return true;
        customOption!.UpdateSelection(__instance.GetInt());
        return false;
    }

    [HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.UpdateValue))]
    [HarmonyPrefix]
    private static bool ToggleOptionValueUpdatePatch(ToggleOption __instance)
    {
        if (!CustomOption.TryGetOption(__instance, out var customOption)) return true;
        customOption!.UpdateSelection(__instance.GetBool());
        return false;
    }

    [HarmonyPatch(typeof(NumberOption), InitializeName)]
    [HarmonyPatch(typeof(StringOption), InitializeName)]
    [HarmonyPatch(typeof(ToggleOption), InitializeName)]
    [HarmonyPostfix]
    private static void OptionNamePatch(OptionBehaviour __instance)
    {
        if (!CustomOption.TryGetOption(__instance, out var customOption)) return;
        var titleText = __instance.transform.FindChild("Title Text").GetComponent<TextMeshPro>();
        titleText.text = customOption!.Name();
        if (__instance is ToggleOption toggle)
        {
            toggle.CheckMark.enabled = customOption.GetBool();
        }
        else if (__instance is NumberOption number)
        {
            number.oldValue = float.MinValue;
            number.Value = (float)customOption.GetDynamicValue();
        }
        else if (__instance is StringOption option)
        {
            option.Value = customOption.Selection;
        }
    }

    [HarmonyPatch(typeof(StringOption), nameof(StringOption.FixedUpdate))]
    [HarmonyPrefix]
    private static bool StringOptionValueTextPatch(StringOption __instance)
    {
        if (!CustomOption.TryGetOption(__instance, out var customOption)) return true;
        if (__instance.oldValue != __instance.Value)
        {
            __instance.oldValue = __instance.Value;
            __instance.ValueText.text = customOption!.GetString();
        }

        return false;
    }
}

[HarmonyPatch(typeof(GameSettingMenu), nameof(GameSettingMenu.Close))]
public static class SettingMenuClosePatch
{
    public static void Postfix()
    {
        RoleOptionPatch.CampTabs.Clear();
        RoleOptionPatch.CurrentTab = null;
        RoleOptionPatch.CurrentAdvancedTabFor = null;
        RoleOptionPatch.ScrollerLocationPercent = 0f;

        ControllerManager.Instance.CurrentUiState.MenuName = ""; // Fix player is unmoveable after closing menu
    }
}