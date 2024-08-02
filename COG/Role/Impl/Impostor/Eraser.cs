using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.HManager;
using COG.Role.Impl.Neutral;
using COG.States;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace COG.Role.Impl.Impostor;

public class Eraser : CustomRole, IListener
{
    private readonly CustomButton _eraseButton;

    private readonly CustomOption _initialEraseCooldown, _increaseCooldownAfterErasing, _canEraseImpostors;

    private PlayerControl? _currentPlayer;

    public Eraser() : base(LanguageConfig.Instance.EraserName, Palette.ImpostorRed, CampType.Impostor)
    {
        BaseRoleType = RoleTypes.Impostor;
        ShortDescription = LanguageConfig.Instance.EraserDescription;

        var type = GetTabType(this);
        _initialEraseCooldown = CreateOption(() => LanguageConfig.Instance.EraserInitialEraseCd,
            new FloatOptionValueRule(10f, 5f, 60f, 30f));
        _increaseCooldownAfterErasing = CreateOption(() => LanguageConfig.Instance.EraserIncreaseCdAfterErasing,
            new FloatOptionValueRule(5f, 5f, 15f, 10f));
        _canEraseImpostors = CreateOption(() => LanguageConfig.Instance.EraserCanEraseImpostors,
            new BoolOptionValueRule(false));

        _eraseButton = CustomButton.Create(() =>
            {
                var target = _currentPlayer!;
                var targetRole = target.GetMainRole();

                var camp = targetRole.CampType;

                CustomRole setToRole = camp switch
                {
                    CampType.Crewmate => CustomRoleManager.GetManager().GetTypeRoleInstance<Crewmate.Crewmate>(),
                    CampType.Neutral => CustomRoleManager.GetManager().GetTypeRoleInstance<Jester>(),
                    CampType.Impostor => CustomRoleManager.GetManager().GetTypeRoleInstance<Impostor>(),
                    _ => CustomRoleManager.GetManager().GetTypeRoleInstance<Crewmate.Crewmate>()
                };

                target.SetCustomRole(setToRole);
                var currentCd = _eraseButton!.Cooldown();
                _eraseButton.SetCooldown(currentCd + _increaseCooldownAfterErasing.GetFloat());
            }, () => _eraseButton!.ResetCooldown(),
            () => !(_currentPlayer == null || (!_canEraseImpostors.GetBool() &&
                                               _currentPlayer.GetMainRole().CampType == CampType.Impostor)),
            () => true, ResourceUtils.LoadSprite(ResourcesConstant.EraseButton)!,
            3,
            KeyCode.E, LanguageConfig.Instance.EraseAction, () => _initialEraseCooldown.GetFloat(), -1);
        AddButton(_eraseButton);
    }

    public override void ClearRoleGameData()
    {
        _currentPlayer = null;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnHudUpdate(HudManagerUpdateEvent @event)
    {
        if (!GameStates.InGame) return;
        _currentPlayer = PlayerControl.LocalPlayer.GetClosestPlayer(true, GameUtils.GetGameOptions().KillDistance);
    }

    public override IListener GetListener()
    {
        return this;
    }

    public override CustomRole NewInstance()
    {
        return new Eraser();
    }
}