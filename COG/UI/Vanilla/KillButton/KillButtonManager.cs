using System;
using System.Linq;
using UnityEngine;

namespace COG.UI.Vanilla.KillButton;

/// <summary>
/// 为非内鬼职业的原版击杀按钮管理器，但是有些功能内鬼也能用
/// </summary>
public static class KillButtonManager
{
    public static bool NecessaryKillCondition => PlayerControl.LocalPlayer.IsKillTimerEnabled || PlayerControl.LocalPlayer.ForceKillTimerContinue;

    private static KillButtonSetting _setting = new();

    private static bool _forceShow = false;
    private static Action _afterClick = () => { };
    private static Func<bool> _customCondition = () => true;

    /// <summary>
    /// 是否强制显示击杀按钮
    /// </summary>
    /// <returns></returns>
    public static bool ShouldForceShow() => _forceShow;

    /// <summary>
    /// 切换是否强制可见（内鬼职业若需隐藏击杀按钮则直接用<see cref="ActionButton.Hide"/>）
    /// </summary>
    /// <param name="show">是否显示</param>
    /// <param name="withInitialCooldown">首次使用是否有冷却</param>
    /// <param name="isInitialCooldown10Seconds">首次冷却是否为十秒</param>
    public static void ToggleForceVisible(bool show, bool withInitialCooldown = true, bool isInitialCooldown10Seconds = false)
    {
        HudManager.Instance.KillButton.ToggleVisible(show);

        _forceShow = show;

        if (show)
            PlayerControl.LocalPlayer.SetKillTimer(
                withInitialCooldown
                    ? (isInitialCooldown10Seconds
                        ? 10
                        : (_setting.CustomCooldown < 0
                            ? GameManager.Instance.LogicOptions.GetKillCooldown()
                            : _setting.CustomCooldown))
                    : 0);
    }

    public static void AfterClick() => _afterClick.GetInvocationList().Do(dg => dg.DynamicInvoke());
    public static void AddAfterClick(Action action) => _afterClick += action;

    public static bool CustomCondition() => _customCondition.GetInvocationList().All(dg => dg.DynamicInvoke() == (object)true);
    public static void AddCustomCondition(Func<bool> cond) => _customCondition += cond;

    /// <summary>
    /// 重置冷却（这个类里只有这个方法内鬼职业也能用）
    /// </summary>
    public static void ResetCooldown()
    {
        PlayerControl.LocalPlayer.SetKillTimer(_setting.CustomCooldown < 0
                            ? GameManager.Instance.LogicOptions.GetKillCooldown()
                            : _setting.CustomCooldown);
    }

    public static KillButtonSetting GetSetting() => _setting;
    public static void OverrideSetting(KillButtonSetting setting) => _setting = setting ?? new();

    /// <summary>
    /// 清除所有设置
    /// </summary>
    public static void ClearAll()
    {
        _setting = new();

        _forceShow = false;
        _afterClick = () => { };
        _customCondition = () => true;
    }
}

public class KillButtonSetting
{
    /// <summary>
    /// 是否活着时方可击杀
    /// </summary>
    public bool OnlyUsableWhenAlive { get; } = true;

    /// <summary>
    /// 击杀目标的轮廓线颜色
    /// </summary>
    public Color TargetOutlineColor { get; } = Palette.ImpostorRed;

    /// <summary>
    /// 使用限制（≤0为无限）
    /// </summary>
    public int UsesLimit { get; } = 0;

    /// <summary>
    /// 自定义冷却（≤0为无冷却）
    /// </summary>
    public float CustomCooldown { get; } = float.NegativeInfinity;
}