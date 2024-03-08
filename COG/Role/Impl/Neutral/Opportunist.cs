using System.Linq;
using System.Threading;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Game.CustomWinner;
using COG.Listener;
using COG.Listener.Event.Impl.GSManager;
using COG.Listener.Event.Impl.Player;
using COG.States;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Neutral;

public class Opportunist : Role, IListener
{
    private readonly CustomOption? _killCooldownOption;
    private readonly CustomButton _killButton;

    public Opportunist() : base(LanguageConfig.Instance.OpportunistName, Color.yellow, CampType.Neutral, true)
    {
        Description = LanguageConfig.Instance.OpportunistDescription;
        _killCooldownOption = CustomOption.Create(false, CustomOption.CustomOptionType.Neutral,
            LanguageConfig.Instance.KillCooldown, 45f, 20f, 200f, 1f, MainRoleOption);
        BaseRoleType = RoleTypes.Impostor;
        _killButton = CustomButton.Create(
            () =>
            {
                var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                if (!target) return;
                PlayerControl.LocalPlayer.CmdCheckMurder(target);
            },
            () => _killButton?.ResetCooldown(),
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
            ResourceUtils.LoadSpriteFromResources(ResourcesConstant.GeneralKillButton, 100f)!,
            2,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            (Cooldown)_killCooldownOption!.GetFloat,
            -1
        );
            
        AddButton(_killButton);
    }

    public override IListener GetListener()
    {
        return this;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnMurderPlayer(PlayerMurderEvent @event)
    {
        if (!GameStates.InGame) return;
        var target = @event.Target;
        if (target.GetRoleInstance()!.Id == Id) CustomWinnerManager.UnRegisterCustomWinner(target);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStartWithMovement(GameStartManagerStartEvent @event)
    {
        if (!GameStates.InGame) return;
        var thread = new Thread(() =>
        {
            Thread.Sleep(500);
            CustomWinnerManager.RegisterCustomWinners(PlayerUtils.GetAllPlayers()
                .Where(p => p.GetRoleInstance()!.Id == Id));
        });
        thread.Start();
    }
}