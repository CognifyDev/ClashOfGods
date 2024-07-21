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
using COG.Utils.Coding;
using UnityEngine;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace COG.Role.Impl.Impostor;

[NotUsed]
[WorkInProgress]
public class Eraser : CustomRole, IListener
{
    private readonly CustomButton _eraseButton;

    private PlayerControl? _currentPlayer;

    private readonly CustomOption _initialEraseCooldown, _increaseCooldownAfterErasing, _canEraseImpostors;

    public override void ClearRoleGameData()
    {
        _currentPlayer = null;
    }

    public Eraser() : base(LanguageConfig.Instance.EraserName, Palette.ImpostorRed, CampType.Impostor)
    {
        BaseRoleType = RoleTypes.Impostor;
        Description = LanguageConfig.Instance.EraserDescription;

        var type = ToCustomOption(this);
        _initialEraseCooldown = CustomOption.Create(type, () => LanguageConfig.Instance.EraserInitialEraseCd,
            new FloatOptionValueRule(10f, 5f, 60f, 30f), MainRoleOption);
        _increaseCooldownAfterErasing = CustomOption.Create(type, () => LanguageConfig.Instance.EraserIncreaseCdAfterErasing,
            new FloatOptionValueRule(5f, 5f, 15f, 10f), MainRoleOption);
        _canEraseImpostors = CustomOption.Create(type, () => LanguageConfig.Instance.EraserCanEraseImpostors,
            new BoolOptionValueRule(false), MainRoleOption);

        _eraseButton = CustomButton.Create(() =>
            {
                var target = _currentPlayer!;
                var targetRole = target.GetMainRole();

                var camp = targetRole.CampType;

                CustomRole setToRole = camp switch
                {
                    CampType.Crewmate => CustomRoleManager.GetManager().GetTypeRoleInstance<Crewmate.Crewmate>(),
                    CampType.Neutral => CustomRoleManager.GetManager().GetTypeRoleInstance<Opportunist>(),
                    CampType.Impostor => CustomRoleManager.GetManager().GetTypeRoleInstance<Impostor>(),
                    _ => CustomRoleManager.GetManager().GetTypeRoleInstance<Crewmate.Crewmate>()
                };

                target.SetCustomRole(setToRole);
                var currentCd = _eraseButton!.Cooldown();
                _eraseButton.SetCooldown(currentCd + _increaseCooldownAfterErasing.GetFloat());
            }, () => _eraseButton!.ResetCooldown(),
            () => !(_currentPlayer == null || (!_canEraseImpostors.GetBool() &&
                                               _currentPlayer.GetMainRole().CampType == CampType.Impostor)),
            () => true, ResourceUtils.LoadSpriteFromResources(ResourcesConstant.EraseButton, 100f)!,
            3,
            KeyCode.E, LanguageConfig.Instance.EraseAction, () => _initialEraseCooldown.GetFloat(), -1);
        AddButton(_eraseButton);
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