using System;
using System.Linq;
using UnityEngine;

namespace COG.UI.Vanilla.KillButton;

/// <summary>
/// 为所有职业的原版击杀按钮管理器
/// </summary>
public static class KillButtonManager
{
    public static bool NecessaryKillCondition => PlayerControl.LocalPlayer.IsKillTimerEnabled || PlayerControl.LocalPlayer.ForceKillTimerContinue;

    private static KillButtonSetting _setting = new();

    /// <summary>
    /// 是否强制显示击杀按钮
    /// </summary>
    /// <returns></returns>
    public static bool ShouldForceShow() => _setting.ForceShow();

    /// <summary>
    /// 切换是否强制可见
    /// </summary>
    /// <param name="show">是否显示</param>
    /// <param name="withInitialCooldown">首次使用是否有冷却</param>
    /// <param name="isInitialCooldown10Seconds">首次冷却是否为十秒</param>
    public static void ForceToggleVisible(bool show, bool withInitialCooldown = true, bool isInitialCooldown10Seconds = false)
    {
        OverrideSetting(new()
        {
            ForceShow = () => show,
            InitialCooldown = withInitialCooldown
                ? (isInitialCooldown10Seconds
                    ? 10
                    : GameManager.Instance.LogicOptions.GetKillCooldown()
                  )
                : 0
        });
    }


    /// <summary>
    /// 重置冷却
    /// </summary>
    public static void ResetCooldown()
    {
        PlayerControl.LocalPlayer.SetKillTimer(_setting.CustomCooldown < 0
                            ? GameManager.Instance.LogicOptions.GetKillCooldown()
                            : _setting.CustomCooldown);
    }

    public static KillButtonSetting GetSetting() => _setting;
    public static void OverrideSetting(KillButtonSetting setting)
    {
        _setting = setting ?? new();

        var visible = setting!.ForceShow();
        HudManager.Instance.KillButton.ToggleVisible(visible);

        if (visible)
        {
            if (setting.InitialCooldown >= 0)
                PlayerControl.LocalPlayer.SetKillTimer(setting.InitialCooldown);
            else
                ResetCooldown();
        }
    }

    /// <summary>
    /// 清除所有设置
    /// </summary>
    public static void ClearAll()
    {
        _setting = new();
    }
}

public class KillButtonSetting
{
    /// <summary>
    /// 是否活着时方可击杀
    /// </summary>
    public bool OnlyUsableWhenAlive { get; set; } = true;

    /// <summary>
    /// 击杀目标的轮廓线颜色
    /// </summary>
    public Color TargetOutlineColor { get; set; } = Palette.ImpostorRed;

    /// <summary>
    /// 使用限制（≤0为无限）
    /// </summary>
    public int UsesLimit { get; set; } = 0;

    /// <summary>
    /// 自定义冷却（＜0为使用游戏默认值）
    /// </summary>
    public float CustomCooldown { get; set; } = float.NegativeInfinity;

    /// <summary>
    /// 是否强制显示
    /// </summary>
    public Func<bool> ForceShow { get; set; } = () => false;

    public float InitialCooldown { get; set; } = float.NegativeInfinity;

    private Action _afterClick = () => { };
    private Func<bool> _customCondition = () => true;

    public void AfterClick() => _afterClick.GetInvocationList().Do(dg => dg.DynamicInvoke());
    public void AddAfterClick(Action action) => _afterClick += action;

    public bool CustomCondition() => _customCondition.GetInvocationList().All(dg => dg.DynamicInvoke() == (object)true);
    public void AddCustomCondition(Func<bool> cond) => _customCondition += cond;
}