﻿using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;

namespace COG.Role.Impl.Impostor;

public class Stabber : CustomRole
{
    private CustomButton DispatchButton { get; }
    
    private CustomOption MaxUseTime { get; }

    private int _killedTimes;

    private PlayerControl? _target;
    
    public Stabber() : base()
    {
        BaseRoleType = RoleTypes.Impostor;
        CanKill = true;
        CanVent = true;
        CanSabotage = true;

        MaxUseTime = CreateOption(() => LanguageConfig.Instance.MaxUseTime, new FloatOptionValueRule(1F, 1F, 15F, 2F));
        
        DispatchButton = CustomButton.Of(
            "stabber-dispatch",
            () =>
            {
                _target!.RpcMurderAndModifyKillAnimation(_target!, PlayerControl.LocalPlayer, true);
                _killedTimes ++;
            },
            () => DispatchButton?.ResetCooldown(),
            () => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target),
            () =>  _killedTimes < MaxUseTime.GetFloat(),
            ResourceUtils.LoadSprite(ResourceConstant.DispatchButton)!,
            2,
            LanguageConfig.Instance.DispatchAction,
            () => GameUtils.GetGameOptions().KillCooldown,
            0
        );

        DefaultKillButtonSetting.AddCustomCondition(() => _killedTimes >= MaxUseTime.GetFloat());
        
        AddButton(DispatchButton);
    }

    public override void ClearRoleGameData()
    {
        _killedTimes = 0;
    }
}