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
        CanVent = true;
        CanSabotage = true;

        CleanBodyCd = CreateOption(() => LanguageConfig.Instance.CleanBodyCooldown,
            new FloatOptionValueRule(10f, 5f, 60f, 30f));

        CleanBodyButton = CustomButton.Of(
            () =>
            {
                var body = PlayerUtils.GetClosestBody();
                if (!body) return;
                body!.RpcCleanDeadBody();
                ButtonConstant.KillButton.ResetCooldown();
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

        AddButton(CleanBodyButton);
    }

    private CustomOption CleanBodyCd { get; }
    private CustomButton CleanBodyButton { get; }

    public override IListener GetListener()
    {
        return this;
    }
}