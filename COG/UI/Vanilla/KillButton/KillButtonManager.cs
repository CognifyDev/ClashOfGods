using System;
using System.Linq;
using UnityEngine;

namespace COG.UI.Vanilla.KillButton;

/// <summary>
/// Ϊ���ڹ�ְҵ��ԭ���ɱ��ť��������������Щ�����ڹ�Ҳ����
/// </summary>
public static class KillButtonManager
{
    public static bool NecessaryKillCondition => PlayerControl.LocalPlayer.IsKillTimerEnabled || PlayerControl.LocalPlayer.ForceKillTimerContinue;

    private static KillButtonSetting _setting = new();

    private static bool _forceShow = false;
    private static Action _afterClick = () => { };
    private static Func<bool> _customCondition = () => true;

    /// <summary>
    /// �Ƿ�ǿ����ʾ��ɱ��ť
    /// </summary>
    /// <returns></returns>
    public static bool ShouldForceShow() => _forceShow;

    /// <summary>
    /// �л��Ƿ�ǿ�ƿɼ����ڹ�ְҵ�������ػ�ɱ��ť��ֱ����<see cref="ActionButton.Hide"/>��
    /// </summary>
    /// <param name="show">�Ƿ���ʾ</param>
    /// <param name="withInitialCooldown">�״�ʹ���Ƿ�����ȴ</param>
    /// <param name="isInitialCooldown10Seconds">�״���ȴ�Ƿ�Ϊʮ��</param>
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
    /// ������ȴ���������ֻ����������ڹ�ְҵҲ���ã�
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
    /// �����������
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
    /// �Ƿ����ʱ���ɻ�ɱ
    /// </summary>
    public bool OnlyUsableWhenAlive { get; } = true;

    /// <summary>
    /// ��ɱĿ�����������ɫ
    /// </summary>
    public Color TargetOutlineColor { get; } = Palette.ImpostorRed;

    /// <summary>
    /// ʹ�����ƣ���0Ϊ���ޣ�
    /// </summary>
    public int UsesLimit { get; } = 0;

    /// <summary>
    /// �Զ�����ȴ����0Ϊ����ȴ��
    /// </summary>
    public float CustomCooldown { get; } = float.NegativeInfinity;
}