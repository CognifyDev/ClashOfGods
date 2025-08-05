using System;
using System.Linq;
using UnityEngine;

namespace COG.UI.Vanilla.KillButton;

public class KillButtonSetting
{
    /// <summary>
    /// �Ƿ����ʱ���ɻ�ɱ
    /// </summary>
    public bool OnlyUsableWhenAlive { get; set; } = true;

    /// <summary>
    /// ��ɱĿ�����������ɫ
    /// </summary>
    public Color TargetOutlineColor { get; set; } = Palette.ImpostorRed;

    /// <summary>
    /// ʹ�����ƣ���0Ϊ���ޣ�
    /// </summary>
    public int UsesLimit { get; set; } = 0;

    /// <summary>
    /// �Զ�����ȴ����0Ϊʹ����ϷĬ��ֵ��
    /// </summary>
    public Func<float> CustomCooldown { get; set; } = () => float.NegativeInfinity;

    /// <summary>
    /// �Ƿ�ǿ����ʾ
    /// </summary>
    public Func<bool> ForceShow { get; set; } = () => false;

    /// <summary>
    /// ��ʼ��ȴ����0Ϊʹ���Զ������Ϸ��ɱ��ȴ��
    /// </summary>
    public float InitialCooldown { get; set; } = float.NegativeInfinity;

    public int RemainingUses { get; set; }

    public string? ExtraRpcMessage { get; set; } = null;

    private Action _afterClick = () => { };
    private Func<bool> _customCondition = () => true;
    private Func<global::KillButton, PlayerControl> _beforeMurder = b => b.currentTarget;

    public void AfterClick() => _afterClick.GetInvocationList().Do(dg => dg.DynamicInvoke());
    public void AddAfterClick(Action action) => _afterClick += action;

    public bool CustomCondition() => _customCondition.GetInvocationList().All(dg => (bool)dg.DynamicInvoke()!);
    public void AddCustomCondition(Func<bool> cond) => _customCondition += cond;

    public PlayerControl BeforeMurder() => _beforeMurder(HudManager.Instance.KillButton);
    public void SetBeforeMurder(Func<global::KillButton, PlayerControl> beforeMurder) => _beforeMurder = beforeMurder;

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
            ExtraRpcMessage = ExtraRpcMessage,
            _afterClick = _afterClick,
            _customCondition = _customCondition,
            _beforeMurder = _beforeMurder
        };
    }
}