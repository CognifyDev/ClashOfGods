using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using System;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Sheriff : CustomRole, IListener
{
    public Sheriff() : base(Color.yellow, CampType.Crewmate)
    {
        BaseRoleType = RoleTypes.Crewmate; 
        
        SheriffKillCd = CreateOption(() => LanguageConfig.Instance.KillCooldown,
                new FloatOptionValueRule(10f, 5f, 60f, 30f, NumberSuffixes.Seconds));

        SheriffKillButton = CustomButton.Of(
            "sheriff-kill",
            () =>
            {
                var localData = PlayerControl.LocalPlayer.Data;
                if (_currentTarget!.GetMainRole().CampType != CampType.Crewmate)
                {
                    PlayerControl.LocalPlayer.CmdCheckMurder(_currentTarget);
                }
                else
                {
                    _ = new DeadPlayer(DateTime.Now, CustomDeathReason.Misfire, localData, localData);
                    PlayerControl.LocalPlayer.CmdCheckMurder(PlayerControl.LocalPlayer);
                }
            },
            () => SheriffKillButton?.ResetCooldown(),
            () => PlayerUtils.CheckClosestTargetInKillDistance(out _currentTarget),
            () => true,
            ResourceUtils.LoadSprite(ResourcesConstant.GeneralKillButton)!,
            2,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            () => SheriffKillCd!.GetFloat(),
            -1
        );

        AddButton(SheriffKillButton);
    }

    private CustomOption SheriffKillCd { get; }
    private CustomButton SheriffKillButton { get; }

    private PlayerControl? _currentTarget;
}