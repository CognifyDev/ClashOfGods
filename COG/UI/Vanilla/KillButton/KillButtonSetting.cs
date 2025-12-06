using System;
using System.Linq;
using UnityEngine;

namespace COG.UI.Vanilla.KillButton;

public class KillButtonSetting
{
    private Action _afterClick = () => { };
    private Func<global::KillButton, PlayerControl> _beforeMurder = b => b.currentTarget;
    private Func<bool> _customCondition = () => true;

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
    public int UsesLimit { get; set; }

    /// <summary>
    /// 自定义冷却（≤0为无冷却）
    /// </summary>
    public Func<float> CustomCooldown { get; set; } = () => float.NegativeInfinity;

    /// <summary>
    /// 强制展示
    /// </summary>
    public Func<bool> ForceShow { get; set; } = () => false;

    /// <summary>
    /// 初始冷却
    /// </summary>
    public float InitialCooldown { get; set; } = float.NegativeInfinity;

    public int RemainingUses { get; set; }

    public void AfterClick()
    {
        _afterClick.GetInvocationList().Do(dg => dg.DynamicInvoke());
    }

    public void AddAfterClick(Action action)
    {
        _afterClick += action;
    }

    public bool CustomCondition()
    {
        return _customCondition.GetInvocationList().All(dg => (bool)dg.DynamicInvoke()!);
    }

    public void AddCustomCondition(Func<bool> cond)
    {
        _customCondition += cond;
    }

    public PlayerControl BeforeMurder()
    {
        return _beforeMurder(HudManager.Instance.KillButton);
    }

    public void SetBeforeMurder(Func<global::KillButton, PlayerControl> beforeMurder)
    {
        _beforeMurder = beforeMurder;
    }

    public KillButtonSetting Clone()
    {
        return new KillButtonSetting
        {
            OnlyUsableWhenAlive = OnlyUsableWhenAlive,
            TargetOutlineColor = TargetOutlineColor,
            UsesLimit = UsesLimit,
            CustomCooldown = CustomCooldown,
            ForceShow = ForceShow,
            InitialCooldown = InitialCooldown,
            RemainingUses = RemainingUses,
            _afterClick = _afterClick,
            _customCondition = _customCondition,
            _beforeMurder = _beforeMurder
        };
    }
}