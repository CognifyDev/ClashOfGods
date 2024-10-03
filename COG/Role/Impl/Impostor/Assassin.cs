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

        MaxUseTime = CreateOption(() => LanguageConfig.Instance.MaxUseTime, new IntOptionValueRule(1, 1, 15, 2));
        
        DispatchButton = CustomButton.Create(
            () =>
            {
                var player = PlayerControl.LocalPlayer.GetClosestPlayer();
                if (player == null)
                {
                    return;
                }
                
                player.RpcMurderPlayer(player, true);
                _hasBeenKilled ++;
            },
            () => DispatchButton?.ResetCooldown(),
            () => PlayerControl.LocalPlayer.GetClosestPlayer() != null && _hasBeenKilled < MaxUseTime.GetInt(),
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