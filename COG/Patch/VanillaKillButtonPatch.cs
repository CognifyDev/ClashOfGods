using System.Linq;
using COG.Game.Events;
using COG.Utils;
using Il2CppSystem;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch]
internal static class VanillaKillButtonPatch
{
    public static bool IsHudActive { get; private set; } = true;
    public static bool ActiveLastFrame { get; set; }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    private static void KillButtonUpdatePatch(PlayerControl __instance)
    {
        if (!__instance.AmOwner) return; // Only local player
        if (!GameStates.InRealGame) return;

        __instance.Data.Role.CanUseKillButton = true;

        var setting = __instance.GetKillButtonSetting();
        var killButton = HudManager.Instance.KillButton;

        if (setting == null) return;

        var aliveUsable = setting.OnlyUsableWhenAlive && __instance.IsAlive();
        var show = setting != null && setting.ForceShow() && IsHudActive && aliveUsable;

        killButton.ToggleVisible(show);

        ActiveLastFrame = show;

        if (!show) return;

        // never log here since it will fill log file with trash

        killButton.OverrideText(TranslationController.Instance.GetString(StringNames.KillLabel) +
                                (show && setting!.UsesLimit > 0 ? $" ({setting.RemainingUses})" : ""));

        if ((PlayerControl.LocalPlayer.IsKillTimerEnabled || PlayerControl.LocalPlayer.ForceKillTimerContinue) &&
            IsHudActive) // Prevent meeting kill (as we canceled vanilla murder check)
        {
            if (!aliveUsable || (setting!.UsesLimit > 0 && setting.RemainingUses <= 0))
            {
                killButton.SetTarget(null);
                return;
            }

            if (setting.CustomCondition())
            {
                PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out var player);
                if (!player) return;

                killButton.SetTarget(player);
                player!.cosmetics.SetOutline(true, new Nullable<Color>(setting.TargetOutlineColor));
            }
        }
    }

    public static void Initialize()
    {
        Main.Logger.LogInfo("Button Initialization");
        PlayerControl.LocalPlayer.ResetKillCooldown();
    }

    [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
    [HarmonyPrefix]
    private static bool ClickPatch(KillButton __instance)
    {
        if (__instance.isActiveAndEnabled
            && __instance.currentTarget
            && !__instance.isCoolingDown
            && PlayerControl.LocalPlayer.CanMove)
        {
            var settings = PlayerControl.LocalPlayer.GetSubRoles()
                .Concat(PlayerControl.LocalPlayer.GetMainRole().ToSingleElementArray())
                .Select(r => r.CurrentKillButtonSetting);

            var activatedSettings = settings.Where(s => s.ForceShow());
            if (!activatedSettings.Any()) return false;
            var setting = activatedSettings.First();

            if (setting.OnlyUsableWhenAlive && !PlayerControl.LocalPlayer.IsAlive())
                return false;

            var dead = setting.BeforeMurder();
            var extraMessage = setting.ExtraRpcMessage;

            __instance.SetTarget(null);

            if (dead)
            {
                PlayerControl.LocalPlayer.ResetKillCooldown();

                if (setting.UsesLimit > 0)
                    setting.RemainingUses--;

                setting.AfterClick();
            }
        }

        return false;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour),
        typeof(bool))]
    [HarmonyPostfix]
    private static void SetHudActivePatch([HarmonyArgument(2)] bool isActive)
    {
        IsHudActive = isActive;
    }
}