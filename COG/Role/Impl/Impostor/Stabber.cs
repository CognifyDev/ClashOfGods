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

    private int _killedTimes;
    private readonly CustomOption _maxUseTime;

    private PlayerControl? _target;

    public Stabber()
    {
        BaseRoleType = RoleTypes.Impostor;
        CanKill = true;
        CanVent = true;
        CanSabotage = true;

        _maxUseTime = CreateOption(() => LanguageConfig.Instance.MaxUseTime, new IntOptionValueRule(1, 1, 15, 2));

        _dispatchButton = CustomButton.Of(
            "stabber-dispatch",
            () =>
            {
                _target!.RpcMurderAdvanced(new(true, new(false, PlayerControl.LocalPlayer.Data, _target!.Data), _target));
                _killedTimes++;
            },
            () => _dispatchButton?.ResetCooldown(),
            () => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target),
            () => _killedTimes < _maxUseTime.GetInt(),
            ResourceUtils.LoadSprite(ResourceConstant.DispatchButton)!,
            2,
            LanguageConfig.Instance.DispatchAction,
            () => GameUtils.GetGameOptions().KillCooldown,
            0
        );

        DefaultKillButtonSetting.AddCustomCondition(() => _killedTimes >= _maxUseTime.GetInt());

        AddButton(_dispatchButton);
    }

    public override void ClearRoleGameData()
    {
        _killedTimes = 0;
    }
}