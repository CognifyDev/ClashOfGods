using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.UI.Vanilla.KillButton;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

public class Cleaner : CustomRole, IListener
{
    public Cleaner() : base()
    {
        BaseRoleType = RoleTypes.Impostor;
        CanKill = true;
        CanVent = true;
        CanSabotage = true;

        CleanBodyCd = CreateOption(() => LanguageConfig.Instance.CleanBodyCooldown,
            new FloatOptionValueRule(10F, 5F, 60F, 30F, NumberSuffixes.Seconds));

        CleanBodyButton = CustomButton.Of(
            "cleaner-clean",
            () =>
            {
                _body!.RpcHideDeadBody();
                PlayerControl.LocalPlayer.ResetKillCooldown();
            },
            () => CleanBodyButton?.ResetCooldown(),
            () =>
            {
                if (_body) _body!.ClearOutline(); // Clear outline of previous target
                _body = PlayerUtils.GetClosestBody();
                if (_body) _body!.SetOutline(Color);
                return _body;
            },
            () => true,
            ResourceUtils.LoadSprite(ResourceConstant.CleanDeadBodyButton)!,
            2,
            LanguageConfig.Instance.CleanAction,
            () => CleanBodyCd.GetFloat(),
            0
        );

        AddButton(CleanBodyButton);
    }

    private CustomOption CleanBodyCd { get; }
    private CustomButton CleanBodyButton { get; }

    private DeadBody? _body;

    public override IListener GetListener()
    {
        return this;
    }
}