using COG.Config.Impl;
using COG.Constant;
using COG.Game.CustomWinner;
using COG.Listener;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Event.Impl.Player;
using COG.States;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Neutral;

public class Opportunist : CustomRole, IListener
{
    private readonly CustomButton _killButton;
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly CustomOption? _killCooldownOption;

    public Opportunist() : base(LanguageConfig.Instance.OpportunistName, Color.yellow, CampType.Neutral)
    {
        ShortDescription = LanguageConfig.Instance.OpportunistDescription;
        if (ShowInOptions) 
            _killCooldownOption = CreateOption(() => LanguageConfig.Instance.KillCooldown, 
            new FloatOptionValueRule(20f, 5f, 200f, 45f));
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
            ResourceUtils.LoadSprite(ResourcesConstant.GeneralKillButton, 100f)!,
            2,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            () => _killCooldownOption!.GetFloat(),
            -1
        );

        AddButton(_killButton);
    }

    public override IListener GetListener()
    {
        return this;
    }

    public override CustomRole NewInstance()
    {
        return new Opportunist();
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnMurderPlayer(PlayerMurderEvent @event)
    {
        if (!GameStates.InGame) return;
        var target = @event.Target;
        if (target.IsRole(this)) CustomWinnerManager.UnregisterWinningPlayer(target);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnExilePlayer(PlayerExileEndEvent @event) => HandleExile(@event.Player);

    [EventHandler(EventHandlerType.Postfix)]
    public void OnAirshipExilePlayer(PlayerExileEndOnAirshipEvent @event) => HandleExile(@event.Player);
    
    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStartWithMovement(GameStartEvent @event)
    {
        if (!GameStates.InGame) return;
        CustomWinnerManager.RegisterWinningPlayers(Players);
        CustomWinnerManager.SetWinText("OpportunistRoleWinningText");
        CustomWinnerManager.SetWinColor(Color);
    }

    public void HandleExile(PlayerControl exiled)
    {
        if (!exiled) return;
        if (exiled.IsRole(this)) CustomWinnerManager.UnregisterWinningPlayer(exiled);
    }
}