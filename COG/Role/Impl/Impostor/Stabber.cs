using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;

namespace COG.Role.Impl.Impostor;

public class Stabber : CustomRole
{
    public const string ModifyKillAnimMessage = "stabber_kill";

    private CustomButton _dispatchButton;
    private CustomOption _maxUseTime;

    private int _killedTimes;

    private PlayerControl? _target;
    
    public Stabber() : base()
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
                _target!.CmdExtraCheckMurder(_target!, ModifyKillAnimMessage + PlayerControl.LocalPlayer.PlayerId);
                _killedTimes++;
            },
            () => _dispatchButton?.ResetCooldown(),
            () => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target),
            () =>  _killedTimes < _maxUseTime.GetInt(),
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