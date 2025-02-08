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
            () =>
            {
                var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                if (!target) return;

                var localData = PlayerControl.LocalPlayer.Data;
                if (target!.GetMainRole().CampType != CampType.Crewmate)
                {
                    PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
                }
                else
                {
                    _ = new DeadPlayer(DateTime.Now, CustomDeathReason.Misfire, localData, localData);
                    PlayerControl.LocalPlayer.RpcMurderPlayer(PlayerControl.LocalPlayer, true);
                }
            },
            () => SheriffKillButton?.ResetCooldown(),
            () =>
            {
                var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                if (target == null) return false;
                var localPlayer = PlayerControl.LocalPlayer;
                var localLocation = localPlayer.GetTruePosition();
                var targetLocation = target.GetTruePosition();
                var distance = Vector2.Distance(localLocation, targetLocation);
                return GameUtils.GetGameOptions().KillDistance >= distance;
            },
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
}