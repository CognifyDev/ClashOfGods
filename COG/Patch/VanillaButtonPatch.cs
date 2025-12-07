using System.Linq;
using AmongUs.GameOptions;
using COG.Utils;
using COG.Utils.Coding;

namespace COG.Patch;

[WorkInProgress]
[HarmonyPatch]
internal static class VanillaButtonPatch
{
    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.Refresh))]
    [HarmonyPrefix]
    private static bool VanillaSabotageButtonRefreshPatch(SabotageButton __instance)
    {
        if (!GameManager.Instance || !PlayerControl.LocalPlayer)
        {
            __instance.ToggleVisible(false);
            __instance.SetDisabled();
        }
        else if (PlayerControl.LocalPlayer.inVent || !GameManager.Instance.SabotagesEnabled() || PlayerControl.LocalPlayer.petting)
        {
            __instance.ToggleVisible(PlayerControl.LocalPlayer.GetRoles().Any(role => role.CanSabotage) && GameManager.Instance.SabotagesEnabled());
            __instance.SetDisabled();
        }
        else
            __instance.SetEnabled();
        return false;
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool))]
public static class HudActivePatch
{
    [HarmonyPostfix]
    static void Postfix(HudManager __instance, [HarmonyArgument(0)] PlayerControl localPlayer, [HarmonyArgument(1)] RoleBehaviour role, [HarmonyArgument(2)] bool isActive)
    {
        var roles = PlayerControl.LocalPlayer.GetRoles();
        var sabotageFlag = roles.Any(r => r.CanSabotage);
        var ventable = roles.Any(r => r.CanVent);
        __instance.SabotageButton.ToggleVisible(isActive && sabotageFlag);
        __instance.AdminButton.ToggleVisible(isActive && sabotageFlag);
        __instance.ImpostorVentButton.ToggleVisible(isActive && localPlayer.IsAlive() && ventable && GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.HideNSeek);
    }
}