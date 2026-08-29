using System.Linq;
using AmongUs.GameOptions;
using COG.Role;
using COG.States;
using COG.Utils;
using COG.Utils.Coding;

namespace COG.Patch;

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
            return false;
        }

        var canSabotage = false;
        try { canSabotage = PlayerControl.LocalPlayer.GetRoles().Any(role => role.CanSabotage); } catch { }

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

    [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
    [HarmonyPrefix]
    private static bool SabotageButtonDoClickPatch()
    {
        if (!GameManager.Instance || !GameManager.Instance.SabotagesEnabled()) return true;

        var canSabotage = false;
        try { canSabotage = PlayerControl.LocalPlayer.GetRoles().Any(role => role.CanSabotage); } catch { }
        if (!canSabotage) return true;

        HudManager.Instance.ToggleMapVisible(new MapOptions
        {
            Mode = MapOptions.Modes.Sabotage,
            AllowMovementWhileMapOpen = true,
        });
        return false;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.ToggleMapVisible), typeof(MapOptions))]
    [HarmonyPrefix]
    private static bool OnToggleMapVisible(ref MapOptions options)
    {
        if (options.Mode == MapOptions.Modes.Normal && GameStates.InRealGame)
        {
            try
            {
                if (PlayerControl.LocalPlayer.GetRoles().Any(r => r.CanSabotage))
                    options.Mode = MapOptions.Modes.Sabotage;
            }
            catch { }
        }
        return true;
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
            if (!localPlayer || !localPlayer.Data || localPlayer.Data.IsDead) return;

            var roles = localPlayer.GetRoles();
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

/// <summary>
///     Sync vanilla RoleBehaviour fields every frame for the local player.
///     Ensures AffectedByLightAffectors and TasksCountTowardProgress are
///     always correct based on the custom role's camp type.
/// </summary>
[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
public static class VanillaRoleFieldSyncPatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        try
        {
            var player = PlayerControl.LocalPlayer;
            if (!player || !player.Data || player.Data.Role == null) return;

            var mainRole = player.GetMainRole();
            var vanillaRole = player.Data.Role;

            if (vanillaRole != null)
            {
                vanillaRole.AffectedByLightAffectors = mainRole.CampType != CampType.Impostor;
                vanillaRole.TasksCountTowardProgress = mainRole.CampType != CampType.Impostor;
            }
        }
        catch { }
    }
}
