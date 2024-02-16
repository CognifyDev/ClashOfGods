using System.Linq;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Listener;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Neutral;

public class Opportunist : Role, IListener
{
    private readonly CustomOption? _killCooldownOption;
    private readonly CustomButton? _killButton;
    
    public Opportunist() : base(LanguageConfig.Instance.OpportunistName, Color.yellow, CampType.Neutral, true)
    {
        Description = LanguageConfig.Instance.OpportunistDescription;
        _killCooldownOption = CustomOption.Create(false, CustomOption.CustomOptionType.Neutral, 
            LanguageConfig.Instance.KillCooldown, 45f, 20f, 200f, 1f, MainRoleOption);
        if (_killCooldownOption != null)
            _killButton = CustomButton.Create(
                () =>
                {
                    var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                    if (!target) return;
                    PlayerControl.LocalPlayer.CmdCheckMurder(target);
                },
                () => _killButton?.ResetCooldown(),
                couldUse: () =>
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
                ResourceUtils.LoadSpriteFromResources("COG.Resources.InDLL.Images.Buttons.GeneralKill.png", 100f)!,
                row: 2,
                KeyCode.Q,
                LanguageConfig.Instance.KillAction,
                (Cooldown)_killCooldownOption.GetFloat,
                -1
            );
        BaseRoleType = RoleTypes.Crewmate;
    }

    public override IListener GetListener(PlayerControl player) => this;
    
    public void OnMurderPlayer(PlayerControl killer, PlayerControl target)
    {
        if (target.GetRoleInstance()!.Id == Id)
        {
            CustomWinnerManager.UnRegisterCustomWinner(target);
        }
    }

    public void OnGameStartWithMovement(GameManager manager)
    {
        CustomWinnerManager.RegisterCustomWinners(PlayerUtils.GetAllPlayers().Where(p => p.GetRoleInstance()!.Id == Id));
    }
}
