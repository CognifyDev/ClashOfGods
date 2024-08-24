using System.Linq;
using COG.Config.Impl;
using COG.UI.CustomButton;
using COG.Utils;
using UnityEngine;

namespace COG.Constant;

public static class ButtonConstant
{
    public static readonly CustomButton KillButton = CustomButton.Create(
        () =>
        {
            var target = PlayerControl.LocalPlayer.GetClosestPlayer();
            if (target == null) return;
            PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
        },
        () => KillButton.ResetCooldown(),
        () => PlayerControl.LocalPlayer.GetClosestPlayer(true, GameUtils.GetGameOptions().KillDistance) != null,
        () =>
        {
            var role = PlayerControl.LocalPlayer.GetPlayerData();
            if (role == null)
            {
                return false;
            }
            return role.Role.CanKill || role.SubRoles.Any(subRole => subRole.CanKill);
        },
        ResourceUtils.LoadSprite(ResourcesConstant.GeneralKillButton)!,
        1,
        KeyCode.Q,
        LanguageConfig.Instance.KillAction,
        () => GameUtils.GetGameOptions().KillCooldown,
        0);
}