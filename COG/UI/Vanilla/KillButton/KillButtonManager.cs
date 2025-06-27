using System;
using UnityEngine;

namespace COG.UI.Vanilla.KillButton;

public static class KillButtonManager
{
    public static bool NecessaryKillCondition => PlayerControl.LocalPlayer.IsKillTimerEnabled || PlayerControl.LocalPlayer.ForceKillTimerContinue;
    public static bool OnlyUsableWhenAlive { get; set; } = true;
    public static Func<bool> CustomCondition { get; set; } = () => true;
    public static Color TargetOutlineColor { get; set; } = Palette.ImpostorRed;
    public static int UsesLimit { get; set; } = 0;
    public static float CustomCooldown { get; set; } = float.NegativeInfinity;

    private static bool _forceShow = false;
    private static Action _afterClick  = () => { };

    public static bool ShouldForceShow() => _forceShow;
    public static void ToggleForceVisible(bool show, bool withInitialCooldown = true, bool isInitialCooldown10Seconds = false)
    {
        HudManager.Instance.KillButton.ToggleVisible(show);

        _forceShow = show;

        if (show)
            PlayerControl.LocalPlayer.SetKillTimer(
                withInitialCooldown
                    ? (isInitialCooldown10Seconds
                        ? 10
                        : (CustomCooldown < 0
                            ? GameManager.Instance.LogicOptions.GetKillCooldown()
                            : CustomCooldown))
                    : 0);
    }

    public static void AfterClick() => _afterClick.GetInvocationList().Do(dg => dg.DynamicInvoke());
    public static void AddAfterClick(Action action) => _afterClick += action;

    public static void ClearAll()
    {
        OnlyUsableWhenAlive = true;
        CustomCondition = () => true;
        TargetOutlineColor = Palette.ImpostorRed;
        UsesLimit = -1;
        CustomCooldown = float.NegativeInfinity;

        _forceShow = false;
        _afterClick = () => { };
    }
}