using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.Utils;

namespace COG.Role.Impl.Impostor;

public class Stabber : CustomRole
{
    private readonly CustomButton _dispatchButton;
    private readonly CustomOption _maxUseTime;

    private int _killedTimes;

    private PlayerControl? _target;

    public Stabber()
    {
        BaseRoleType = RoleTypes.Impostor;
        CanKill = true;
        CanVent = true;
        CanSabotage = true;

        _maxUseTime = CreateOption(() => LanguageConfig.Instance.MaxUseTime, new FloatOptionValueRule(1, 1, 15, 2));

        _dispatchButton = CustomButton.Builder("stabber-dispatch", ResourceConstant.DispatchButton,
                LanguageConfig.Instance.DispatchAction)
            .OnClick(() =>
            {
                _target!.RpcMurderAdvanced(new AdvancedKillOptions(true,
                    new KillAnimationOptions(false, [], PlayerControl.LocalPlayer.Data,
                        _target!.Data), _target));
                _killedTimes ++;
            })
            .OnMeetingEnds(() => _dispatchButton?.ResetCooldown())
            .CouldUse(() => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target))
            .HasButton(() => _killedTimes < _maxUseTime.GetInt())
            .Cooldown(() => GameUtils.GetGameOptions().KillCooldown)
            .Build();

        DefaultKillButtonSetting.AddCustomCondition(() => _killedTimes >= _maxUseTime.GetInt());

        AddButton(_dispatchButton);
    }

    public override void ClearRoleGameData()
    {
        _killedTimes = 0;
    }
}