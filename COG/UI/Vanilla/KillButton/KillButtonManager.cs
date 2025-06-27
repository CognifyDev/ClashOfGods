using System;
using System.Linq;
using UnityEngine;

namespace COG.UI.Vanilla.KillButton;

/// <summary>
/// Ϊ����ְҵ��ԭ���ɱ��ť������
/// </summary>
public static class KillButtonManager
{
    public static bool NecessaryKillCondition => PlayerControl.LocalPlayer.IsKillTimerEnabled || PlayerControl.LocalPlayer.ForceKillTimerContinue;

    private static KillButtonSetting _setting = new();

    /// <summary>
    /// �Ƿ�ǿ����ʾ��ɱ��ť
    /// </summary>
    /// <returns></returns>
    public static bool ShouldForceShow() => _setting.ForceShow();

    /// <summary>
    /// �л��Ƿ�ǿ�ƿɼ�
    /// </summary>
    /// <param name="show">�Ƿ���ʾ</param>
    /// <param name="withInitialCooldown">�״�ʹ���Ƿ�����ȴ</param>
    /// <param name="isInitialCooldown10Seconds">�״���ȴ�Ƿ�Ϊʮ��</param>
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
    /// ������ȴ
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
    /// �����������
    /// </summary>
    public static void ClearAll()
    {
        _setting = new();
    }
}

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
    public float CustomCooldown { get; set; } = float.NegativeInfinity;

    /// <summary>
    /// �Ƿ�ǿ����ʾ
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