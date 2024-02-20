using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.States;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Sheriff : Role, IListener
{
    private CustomOption? SheriffKillCd { get; }
    private CustomButton SheriffKillButton { get; }
    public Sheriff() : base(LanguageConfig.Instance.SheriffName, Color.yellow, CampType.Crewmate, true)
    {
        BaseRoleType = RoleTypes.Crewmate;
        Description = LanguageConfig.Instance.SheriffDescription;

        if (ShowInOptions)
        {
            SheriffKillCd = CustomOption.Create(false, CustomOption.CustomOptionType.Crewmate, LanguageConfig.Instance.SheriffKillCooldown, 30f, 10f, 60f, 5f, MainRoleOption);
        }

        SheriffKillButton = CustomButton.Create(
            () =>
            {
                var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                if (!target) return;
                PlayerControl.LocalPlayer.CmdCheckMurder(target);
            },
            () => SheriffKillButton?.ResetCooldown(),
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
            (Cooldown)SheriffKillCd!.GetFloat,
            -1
        );

        AddButton(SheriffKillButton);
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnPlayerMurder(PlayerMurderEvent @event)
    {
        if (!GameStates.InGame) return true;
        var killer = @event.Player;
        var target = @event.Target;
        if (killer == null || target == null) return true;
        if (!killer.IsRole(this)) return true;
        if (target.GetRoleInstance()!.CampType != CampType.Crewmate) return true;
        killer.MurderPlayer(killer, GameUtils.DefaultFlag);
        return false;
    }

    public override IListener GetListener() => this;
}