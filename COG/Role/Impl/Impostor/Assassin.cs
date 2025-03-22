using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

public class Assassin : CustomRole
{
    private CustomButton DispatchButton { get; }
    
    private CustomOption MaxUseTime { get; }

    private int _hasBeenKilled;
    
    public Assassin() : base(Palette.ImpostorRed, CampType.Impostor)
    {
        BaseRoleType = RoleTypes.Impostor;
        CanKill = true;
        CanVent = true;
        CanSabotage = true;

        MaxUseTime = CreateOption(() => LanguageConfig.Instance.MaxUseTime, new FloatOptionValueRule(1F, 1F, 15F, 2F));
        
        DispatchButton = CustomButton.Of(
            "assassin-dispatch",
            () =>
            {
                var player = PlayerControl.LocalPlayer.GetClosestPlayer();
                if (player == null)
                {
                    return;
                }

                PlayerControl.LocalPlayer.RpcMurderAndModifyKillAnimation(player, player);
                _hasBeenKilled ++;
            },
            () => DispatchButton?.ResetCooldown(),
            () => PlayerControl.LocalPlayer.GetClosestPlayer() != null && _hasBeenKilled < MaxUseTime.GetFloat(),
            () => true,
            ResourceUtils.LoadSprite(ResourcesConstant.DispatchButton)!,
            2,
            KeyCode.C,
            LanguageConfig.Instance.DispatchAction,
            () => GameUtils.GetGameOptions().KillCooldown,
            0
        );
        
        AddButton(DispatchButton);
    }

    public override void ClearRoleGameData()
    {
        _hasBeenKilled = 0;
    }
}