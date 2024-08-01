using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.States;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Sheriff : CustomRole, IListener
{
    public Sheriff() : base(LanguageConfig.Instance.SheriffName, Color.yellow, CampType.Crewmate)
    {
        BaseRoleType = RoleTypes.Crewmate;
        ShortDescription = LanguageConfig.Instance.SheriffDescription;

        if (ShowInOptions)
            SheriffKillCd = CreateOption(() => LanguageConfig.Instance.SheriffKillCooldown, new FloatOptionValueRule(10f, 5f, 60f, 30f));

        SheriffKillButton = CustomButton.Create(
            () =>
            {
                var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                if (!target) return;
                PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
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
            ResourceUtils.LoadSprite(ResourcesConstant.GeneralKillButton, 100f)!,
            2,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            () => SheriffKillCd!.GetFloat(),
            -1
        );

        AddButton(SheriffKillButton);
    }

    private CustomOption? SheriffKillCd { get; }
    private CustomButton SheriffKillButton { get; }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnPlayerMurder(PlayerMurderEvent @event)
    {
        if (!GameStates.InGame) return true;
        var killer = @event.Player;
        var target = @event.Target;
        if (killer == null || target == null) return true;
        if (!killer.IsRole(this)) return true;
        if (target.GetMainRole()!.CampType != CampType.Crewmate) return true;
        killer.RpcMurderPlayer(killer, true);
        return false;
    }

    public override IListener GetListener()
    {
        return this;
    }

    public override CustomRole NewInstance()
    {
        return new Sheriff();
    }
}