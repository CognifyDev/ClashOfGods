using System.Linq;
using COG.Config.Impl;
using COG.Role;
using COG.UI.CustomButton;
using COG.Utils;
using UnityEngine;

namespace COG.Constant;

public static class ButtonConstant
{
    public static readonly CustomButton KillButton = CustomButton.Of(
        () =>
        {
            var target = PlayerControl.LocalPlayer.GetClosestPlayer();
            if (target == null) return;
            PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
        },
        () => KillButton.ResetCooldown(),
        () =>
        {
            var closestPlayer = PlayerControl.LocalPlayer.GetClosestPlayer(true, GameUtils.GetGameOptions().KillDistance);
            if (closestPlayer == null) return false;

            if (closestPlayer.GetMainRole().CampType == CampType.Impostor &&
                PlayerControl.LocalPlayer.GetMainRole().CampType == CampType.Impostor) return false;

            return true;
        },
        () =>
        {
            var role = PlayerControl.LocalPlayer.GetPlayerData();
            if (role == null)
            {
                return false;
            }
            return role.MainRole.CanKill || role.SubRoles.Any(subRole => subRole.CanKill);
        },
        ResourceUtils.LoadSprite(ResourcesConstant.GeneralKillButton)!,
        1,
        KeyCode.Q,
        LanguageConfig.Instance.KillAction,
        () => GameUtils.GetGameOptions().KillCooldown,
        0);
}