using COG.UI.Vanilla.KillButton;
using COG.Utils;
using System;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch]
static class VanillaKillButtonPatch
{
    private static bool _isHudActive = true;

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    static void KillButtonUpdatePatch(PlayerControl __instance)
    {
        if (!__instance.AmOwner) return; // Only local player

        __instance.Data.Role.CanUseKillButton = true;

        var killButton = HudManager.Instance.KillButton;
        killButton.ToggleVisible(KillButtonManager.ShouldForceShow() && _isHudActive);

        if (KillButtonManager.NecessaryKillCondition && _isHudActive) // Prevent meeting kill (as we canceled vanilla murder check)
        {
            if (KillButtonManager.OnlyUsableWhenAlive && !__instance.IsAlive())
            {
                killButton.SetTarget(null);
                return;
            }

            __instance.SetKillTimer(__instance.killTimer - Time.fixedDeltaTime); // This needs CanUseKillButton to be true

            if (KillButtonManager.CustomCondition)
            {
                PlayerUtils.CheckClosestTargetInKillDistance(out var player);
                if (!player) return;

                killButton.SetTarget(player);
                player!.cosmetics.SetOutline(true, new(KillButtonManager.TargetOutlineColor));
            }
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), new Type[] { typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool) })]
    [HarmonyPostfix]
    static void SetHudActivePatch([HarmonyArgument(2)] bool isActive) => _isHudActive = isActive;
}