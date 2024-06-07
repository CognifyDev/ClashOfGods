using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.UI.CustomButton;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

public class Madman : CustomRole, IListener
{
    private CustomButton AnnihilateButton { get; }

    public Madman() : base(LanguageConfig.Instance.MadmanName, Palette.ImpostorRed, CampType.Impostor, true)
    {
        Description = LanguageConfig.Instance.MadmanDescription;
        BaseRoleType = RoleTypes.Impostor;
        CanKill = false;
        CanVent = true;

        AnnihilateButton = CustomButton.Create(
            () =>
            {
                var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                if (target == null) return;
                target.MurderPlayer(target, MurderResultFlags.Succeeded);
            },
            () => AnnihilateButton?.ResetCooldown(),
            () => PlayerControl.LocalPlayer.GetClosestPlayer(true, GameUtils.GetGameOptions().KillDistance) != null,
            () => true,
            ResourceUtils.LoadSpriteFromResources(ResourcesConstant.AnnihilateButton, 100f)!,
            2,
            KeyCode.C,
            LanguageConfig.Instance.AnnihilateAction,
            () => 1f,
            0);

        AddButton(AnnihilateButton);
    }

    public override IListener GetListener()
    {
        return this;
    }
}