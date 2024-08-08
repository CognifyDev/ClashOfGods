using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

public class Cleaner : CustomRole, IListener
{
    public Cleaner() : base(Palette.ImpostorRed, CampType.Impostor)
    {
        BaseRoleType = RoleTypes.Impostor;
        CanKill = true;

        if (ShowInOptions)
            CleanBodyCd = CreateOption(() => LanguageConfig.Instance.CleanBodyCooldown,
                new FloatOptionValueRule(10f, 5f, 60f, 30f));

        CleanBodyButton = CustomButton.Create(
            () =>
            {
                var body = PlayerUtils.GetClosestBody();
                if (!body) return;
                body!.RpcCleanDeadBody();
                KillButton?.ResetCooldown();
            },
            () => CleanBodyButton?.ResetCooldown(),
            () => PlayerUtils.GetClosestBody() != null,
            () => true,
            ResourceUtils.LoadSprite(ResourcesConstant.CleanDeadBodyButton)!,
            2,
            KeyCode.C,
            LanguageConfig.Instance.CleanAction,
            () => CleanBodyCd?.GetFloat() ?? 30f,
            0
        );

        KillButton = CustomButton.Create(
            () =>
            {
                PlayerControl.LocalPlayer.CmdCheckMurder(ClosestTarget);
                CleanBodyButton.ResetCooldown();
            },
            () => KillButton?.ResetCooldown(),
            () =>
            {
                var target = ClosestTarget = PlayerControl.LocalPlayer;
                if (target == null) return false;
                var localPlayer = PlayerControl.LocalPlayer;
                var localLocation = localPlayer.GetTruePosition();
                var targetLocation = target.GetTruePosition();
                var distance = Vector2.Distance(localLocation, targetLocation);
                return GameUtils.GetGameOptions().KillDistance >= distance;
            },
            () => true,
            ResourceUtils.LoadSprite(ResourcesConstant.GeneralKillButton)!,
            1,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            () => CleanBodyCd?.GetFloat() ?? 30f,
            -1
        );

        AddButton(CleanBodyButton);
    }

    private CustomOption? CleanBodyCd { get; }
    private CustomButton CleanBodyButton { get; }
    private CustomButton KillButton { get; }
    private static PlayerControl? ClosestTarget { get; set; }

    public override IListener GetListener()
    {
        return this;
    }

    public override CustomRole NewInstance()
    {
        return new Cleaner();
    }
}