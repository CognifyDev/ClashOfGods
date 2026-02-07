using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.Utils;

namespace COG.Role.Impl.Impostor;

public class Cleaner : CustomRole, IListener
{
    private DeadBody? _body;

    public Cleaner()
    {
        BaseRoleType = RoleTypes.Impostor;
        CanKill = true;
        CanVent = true;
        CanSabotage = true;

        CleanBodyCd = CreateOption(() => LanguageConfig.Instance.CleanBodyCooldown,
            new FloatOptionValueRule(10F, 5F, 60F, 30F, NumberSuffixes.Seconds));

        CleanBodyButton = CustomButton.Builder("cleaner-clean",
                ResourceConstant.CleanDeadBodyButton, LanguageConfig.Instance.CleanAction)
            .OnClick(() =>
            {
                _body!.RpcHideDeadBody();
                PlayerControl.LocalPlayer.ResetKillCooldown();
            })
            .OnMeetingEnds(() => CleanBodyButton?.ResetCooldown())
            .CouldUse(() =>
            {
                if (_body) _body!.ClearOutline(); // Clear outline of previous target
                _body = PlayerUtils.GetClosestBody();
                if (_body) _body!.SetOutline(Color);
                return _body;
            })
            .Cooldown(CleanBodyCd.GetFloat)
            .Build();

        AddButton(CleanBodyButton);
    }

    private CustomOption CleanBodyCd { get; }
    private CustomButton CleanBodyButton { get; }

    public override IListener GetListener()
    {
        return this;
    }
}