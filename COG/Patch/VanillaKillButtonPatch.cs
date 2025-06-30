using COG.Role;
using COG.States;
using COG.UI.Vanilla.KillButton;
using COG.Utils;
using System;
using System.Linq;
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
        if (!GameStates.InRealGame) return;

        //KillButtonManager.OverrideSetting(__instance.GetMainRole().KillButtonSetting);

        __instance.Data.Role.CanUseKillButton = true;

        var killButton = HudManager.Instance.KillButton;
        //var setting = KillButtonManager.GetSetting();
        var settings = __instance.GetSubRoles().Concat(__instance.GetMainRole().ToSingleElementArray()).Select(r => r.KillButtonSetting);
        
        killButton.ToggleVisible(settings.Any(r => r.ForceShow()) && _isHudActive);

        var activatedSettings = settings.Where(s => s.ForceShow());
        if (activatedSettings.Count() == 0) return;
        var setting = activatedSettings.First(); // there should be one settings active

        // never log here since it will fill log file with trash

        if (setting.ForceShow() && setting.UsesLimit > 0)
            killButton.OverrideText(TranslationController.Instance.GetString(StringNames.KillLabel) + $" ({setting.RemainingUses})");

        if (KillButtonManager.NecessaryKillCondition && _isHudActive) // Prevent meeting kill (as we canceled vanilla murder check)
        {
            if ((setting.OnlyUsableWhenAlive && !__instance.IsAlive()) || (setting.UsesLimit > 0 && setting.RemainingUses <= 0))
            {
                killButton.SetTarget(null);
                return;
            }

            if (setting.CustomCondition())
            {
                PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out var player);
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
            var settings = PlayerControl.LocalPlayer.GetSubRoles().Concat(PlayerControl.LocalPlayer.GetMainRole().ToSingleElementArray()).Select(r => r.KillButtonSetting);

            __instance.ToggleVisible(settings.Any(r => r.ForceShow()) && _isHudActive);

            var activatedSettings = settings.Where(s => s.ForceShow());
            if (activatedSettings.Count() == 0) return false;
            var setting = activatedSettings.First();

            if (setting.OnlyUsableWhenAlive && !PlayerControl.LocalPlayer.IsAlive())
                return false;

            PlayerControl.LocalPlayer.CmdCheckMurder(setting.BeforeMurder());
            __instance.SetTarget(null);

            KillButtonManager.ResetCooldown();

            if (setting.UsesLimit > 0)
                setting.RemainingUses--;

            setting.AfterClick();
        }

        return false;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), new Type[] { typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool) })]
    [HarmonyPostfix]
    static void SetHudActivePatch([HarmonyArgument(2)] bool isActive) => _isHudActive = isActive;
}