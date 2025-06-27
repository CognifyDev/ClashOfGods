using COG.Role;
using COG.States;
using COG.UI.Vanilla.KillButton;
using COG.Utils;
using System;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch]
static class VanillaKillButtonPatch
{
    private static bool _isHudActive = true;
    private static int _remainingUses = -1;

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    static void KillButtonUpdatePatch(PlayerControl __instance)
    {
        if (!__instance.AmOwner) return; // Only local player
        if (!GameStates.InRealGame) return;
        if (__instance.GetMainRole().CampType != CampType.Impostor) return; // Not for impostor

        __instance.Data.Role.CanUseKillButton = true;

        var killButton = HudManager.Instance.KillButton;
        var setting = KillButtonManager.GetSetting();

        if (KillButtonManager.ShouldForceShow() && setting.UsesLimit > 0)
            killButton.OverrideText(TranslationController.Instance.GetString(StringNames.KillLabel) + $"({_remainingUses}/{setting.UsesLimit})");

        killButton.ToggleVisible(KillButtonManager.ShouldForceShow() && _isHudActive);

        if (KillButtonManager.NecessaryKillCondition && _isHudActive) // Prevent meeting kill (as we canceled vanilla murder check)
        {
            if ((setting.OnlyUsableWhenAlive && !__instance.IsAlive()) || (setting.UsesLimit > 0 && _remainingUses <= 0))
            {
                killButton.SetTarget(null);
                return;
            }

            __instance.SetKillTimer(__instance.killTimer - Time.fixedDeltaTime); // This requires CanUseKillButton to be true

            if (KillButtonManager.CustomCondition())
            {
                PlayerUtils.CheckClosestTargetInKillDistance(out var player);
                if (!player) return;

                killButton.SetTarget(player);
                player!.cosmetics.SetOutline(true, new(setting.TargetOutlineColor));
            }
        }
    }

    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    [HarmonyPrefix]
    static bool ClickPatch(KillButton __instance)
    {
        if (__instance.isActiveAndEnabled
            && __instance.currentTarget
            && !__instance.isCoolingDown
            && PlayerControl.LocalPlayer.CanMove)
        {
            var setting = KillButtonManager.GetSetting();

            if (setting.OnlyUsableWhenAlive && !PlayerControl.LocalPlayer.IsAlive())
                return false;

            PlayerControl.LocalPlayer.CmdCheckMurder(__instance.currentTarget);
            __instance.SetTarget(null);

            KillButtonManager.ResetCooldown();

            if (setting.UsesLimit > 0)
                _remainingUses--;

            KillButtonManager.AfterClick();
        }

        return false;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), new Type[] { typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool) })]
    [HarmonyPostfix]
    static void SetHudActivePatch([HarmonyArgument(2)] bool isActive) => _isHudActive = isActive;
}