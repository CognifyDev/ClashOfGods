using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.UI.CustomButtons;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Sheriff : Role, IListener
{
    public Sheriff() : base(LanguageConfig.Instance.SheriffName, Color.yellow, CampType.Crewmate, true)
    {
        BaseRoleType = RoleTypes.Crewmate;
        Description = LanguageConfig.Instance.SheriffDescription;
        var killButton = CustomButton.Create(
            () => {}, 
            () => {}, 
            () => false, 
            () => true, 
            ResourceUtils.LoadSpriteFromResources("COG.Resources.InDLL.Images.Buttons.GeneralKill.png", 100f)!,
            CustomButton.ButtonPositions.UpperRowRight,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            15f,
            -1
        );
        AddButton(killButton);
    }

    public bool OnPlayerMurder(PlayerControl killer, PlayerControl target)
    {
        if (killer == null || target == null) return true;
        if (!killer.GetRoleInstance()!.Name.Equals(Name)) return true;
        if (target.GetRoleInstance()!.CampType == CampType.Crewmate)
            killer.MurderPlayer(killer);

        return true;
    }

    public override IListener GetListener(PlayerControl player)
    {
        return this;
    }
}