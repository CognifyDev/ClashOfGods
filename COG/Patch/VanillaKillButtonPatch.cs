using COG.Game.Events;
using COG.Rpc;
using COG.Utils;
using System;
using System.Linq;

namespace COG.Patch;

[HarmonyPatch]
static class VanillaKillButtonPatch
{
    public static bool IsHudActive { get; private set; } = true;
    public static bool ActiveLastFrame { get; set; } = false;

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    [HarmonyPostfix]
    static void KillButtonUpdatePatch(PlayerControl __instance)
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

        killButton.OverrideText(TranslationController.Instance.GetString(StringNames.KillLabel) + (show && setting!.UsesLimit > 0 ? $" ({setting.RemainingUses})" : ""));

        if ((PlayerControl.LocalPlayer.IsKillTimerEnabled || PlayerControl.LocalPlayer.ForceKillTimerContinue) && IsHudActive) // Prevent meeting kill (as we canceled vanilla murder check)
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
                player!.cosmetics.SetOutline(true, new(setting.TargetOutlineColor));
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
    static bool ClickPatch(KillButton __instance)
    {
        if (__instance.isActiveAndEnabled
            && __instance.currentTarget
            && !__instance.isCoolingDown
            && PlayerControl.LocalPlayer.CanMove)
        {
            var settings = PlayerControl.LocalPlayer.GetSubRoles().Concat(PlayerControl.LocalPlayer.GetMainRole().ToSingleElementArray()).Select(r => r.CurrentKillButtonSetting);

            var activatedSettings = settings.Where(s => s.ForceShow());
            if (!activatedSettings.Any()) return false;
            var setting = activatedSettings.First();

            if (setting.OnlyUsableWhenAlive && !PlayerControl.LocalPlayer.IsAlive())
                return false;

            var dead = setting.BeforeMurder();
            var extraMessage = setting.ExtraRpcMessage;

            if (extraMessage == null)
            {
                GameEventPatch.ExtraMessage = null;
                PlayerControl.LocalPlayer.CmdCheckMurder(dead);
            }
            else
            {
                CmdExtraCheckMurder(PlayerControl.LocalPlayer, dead, extraMessage);
            }

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

        void CmdExtraCheckMurder(PlayerControl killer, PlayerControl target, string msg)
        {
            killer.isKilling = true;
            GameEventPatch.ExtraMessage = msg;

            if (AmongUsClient.Instance.AmLocalHost || AmongUsClient.Instance.AmModdedHost)
                killer.CheckMurder(target);

            RpcWriter.Start(RpcCalls.CheckMurder).WriteNetObject(target).Write(msg).Finish();
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.SetHudActive), new Type[] { typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool) })]
    [HarmonyPostfix]
    static void SetHudActivePatch([HarmonyArgument(2)] bool isActive) => IsHudActive = isActive;
}