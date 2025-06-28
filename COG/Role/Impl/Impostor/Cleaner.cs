using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Vanilla.KillButton;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

public class Cleaner : CustomRole, IListener
{
    public Cleaner() : base(Palette.ImpostorRed, CampType.Impostor)
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
                var body = PlayerUtils.GetClosestBody();
                if (!body) return;
                body!.RpcHideDeadBody();
                KillButtonManager.ResetCooldown();
            },
            () => CleanBodyButton?.ResetCooldown(),
            () => PlayerUtils.GetClosestBody(),
            () => true,
            ResourceUtils.LoadSprite(ResourcesConstant.CleanDeadBodyButton)!,
            2,
            KeyCode.C,
            LanguageConfig.Instance.CleanAction,
            () => CleanBodyCd.GetFloat(),
            0
        );

        AddButton(CleanBodyButton);
    }

    private CustomOption CleanBodyCd { get; }
    private CustomButton CleanBodyButton { get; }

    public override IListener GetListener()
    {
        return this;
    }
}