using System.Linq;
using AmongUs.GameOptions;
using COG.Utils;
using COG.Utils.Coding;

namespace COG.Patch;

[HarmonyPatch]
internal static class VanillaButtonPatch
{
    /// <summary>
    ///     Fully replace SabotageButton.Refresh to use our custom role CanSabotage check.
    ///     Based on ExtremeRoles SabotageButtonRefreshPatch.
    /// </summary>
    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.Refresh))]
    [HarmonyPrefix]
    private static bool VanillaSabotageButtonRefreshPatch(SabotageButton __instance)
    {
        if (!GameManager.Instance || !PlayerControl.LocalPlayer)
        {
            __instance.ToggleVisible(false);
            __instance.SetDisabled();
            return false;
        }

        var canSabotage = PlayerControl.LocalPlayer.GetRoles().Any(role => role.CanSabotage);

        if (!canSabotage || !GameManager.Instance.SabotagesEnabled() ||
            PlayerControl.LocalPlayer.Data.IsDead)
        {
            __instance.ToggleVisible(false);
            __instance.SetDisabled();
        }
        else
        {
            __instance.ToggleVisible(true);
            if (PlayerControl.LocalPlayer.inVent || PlayerControl.LocalPlayer.petting)
                __instance.SetDisabled();
            else
                __instance.SetEnabled();
        }
        return false;
    }

    /// <summary>
    ///     Prefix SabotageButton.DoClick to open sabotage map for custom impostor roles.
    ///     Based on ExtremeRoles SabotageButtonDoClickPatch.
    /// </summary>
    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    [HarmonyPrefix]
    private static bool SabotageButtonDoClickPatch()
    {
        if (!GameManager.Instance && GameManager.Instance.SabotagesEnabled()) return true;

        var canSabotage = PlayerControl.LocalPlayer.GetRoles().Any(role => role.CanSabotage);
        if (!canSabotage) return true;

        HudManager.Instance.ToggleMapVisible(new MapOptions
        {
            Mode = MapOptions.Modes.Sabotage,
            AllowMovementWhileMapOpen = true,
        });
        return false;
    }
}

[HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool))]
public static class HudActivePatch
{
    [HarmonyPostfix]
    static void Postfix(HudManager __instance, [HarmonyArgument(0)] PlayerControl localPlayer, [HarmonyArgument(1)] RoleBehaviour role, [HarmonyArgument(2)] bool isActive)
    {
        try
        {
            var roles = PlayerControl.LocalPlayer.GetRoles();
            var sabotageFlag = roles.Any(r => r.CanSabotage);
            var ventable = roles.Any(r => r.CanVent);
            __instance.SabotageButton.ToggleVisible(isActive && sabotageFlag);
            __instance.AdminButton.ToggleVisible(isActive && sabotageFlag);
            __instance.ImpostorVentButton.ToggleVisible(isActive && localPlayer.IsAlive() && ventable && GameOptionsManager.Instance.CurrentGameOptions.GameMode != GameModes.HideNSeek);
        }
        catch
        {
            // Game state not ready yet, silently ignore
        }
    }
}