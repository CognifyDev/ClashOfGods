using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Sheriff : Role, IListener
{
    public CustomOption SheriffKillCD = new();
    public Sheriff() : base(LanguageConfig.Instance.SheriffName, Color.yellow, CampType.Crewmate, true)
    {
        BaseRoleType = RoleTypes.Crewmate;
        Description = LanguageConfig.Instance.SheriffDescription;
        var killButton = CustomButton.Create(
            () =>
            {
                var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                PlayerControl.LocalPlayer.CmdCheckMurder(target);
            },
            () => { },
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
            () => false,
            ResourceUtils.LoadSpriteFromResources("COG.Resources.InDLL.Images.Buttons.GeneralKill.png", 100f)!,
            CustomButton.ButtonPositions.UpperRowRight,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            15f,
            -1
        );
        AddButton(killButton);

        if (ShowInOptions)
        {
            var parentID = MainRoleOption!.ID;
            SheriffKillCD = CustomOption.Create(parentID+1,CustomOption.CustomOptionType.Crewmate,)
        }
    }

    public bool OnPlayerMurder(PlayerControl killer, PlayerControl target)
    {
        if (killer == null || target == null) return true;
        if (!killer.IsRole(this)) return true;
        if (target.GetRoleInstance()!.CampType == CampType.Crewmate)
        {
            killer.MurderPlayer(killer, GameUtils.DefaultFlag);
            return false;
        }
        return true;
    }

    public override IListener GetListener(PlayerControl player)
    {
        return this;
    }
}