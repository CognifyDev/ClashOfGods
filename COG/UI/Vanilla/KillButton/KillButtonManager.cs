using UnityEngine;

namespace COG.UI.Vanilla.KillButton;

public static class KillButtonManager
{
    public static bool NecessaryKillCondition => PlayerControl.LocalPlayer.IsKillTimerEnabled || PlayerControl.LocalPlayer.ForceKillTimerContinue;
    public static bool OnlyUsableWhenAlive { get; set; } = true;
    public static bool CustomCondition { get; set; } = true;
    public static Color TargetOutlineColor { get; set; } = Palette.ImpostorRed;

    private static bool _forceShow = false;

    public static bool ShouldForceShow() => _forceShow;
    public static void ToggleForceVisible(bool show, bool withInitialCooldown = true, bool isInitialCooldown10Seconds = false)
    {
        HudManager.Instance.KillButton.ToggleVisible(show);

        if (show)
            PlayerControl.LocalPlayer.SetKillTimer(
                withInitialCooldown
                    ? (isInitialCooldown10Seconds
                        ? 10
                        : GameManager.Instance.LogicOptions.GetKillCooldown())
                    : 0);
    }
}