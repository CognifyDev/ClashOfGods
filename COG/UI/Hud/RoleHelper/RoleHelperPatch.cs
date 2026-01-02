using System.Collections.Generic;
using System.Linq;
using COG.Role;
using COG.Utils;
using UnityEngine;

namespace COG.UI.Hud.RoleHelper;

public static class RoleHelperManager
{
    private static Dictionary<byte, string> _playerRoleDescriptions = new();
    private static string _localPlayerShower = "";

    public static void UpdateLocalPlayerRoleInfo()
    {
        if (PlayerControl.LocalPlayer == null) return;

        var player = PlayerControl.LocalPlayer;
        var mainRole = player.GetMainRole();
        var subRoles = player.GetSubRoles().ToList();

        if (mainRole == null) return;

        _localPlayerShower = BuildRoleDescription(mainRole, subRoles);

        _playerRoleDescriptions[player.PlayerId] = _localPlayerShower;
    }

    private static string BuildRoleDescription(CustomRole mainRole, List<CustomRole> subRoles)
    {
        var result = "";

        // ��ְҵ
        result += $"<color={mainRole.Color.ToColorHexString()}><size=80%>{mainRole.Name}</size>" +
              $"<b><size=50%>({mainRole.ShortDescription})</size></b></color>" +
              $"<size=40%>:{mainRole.GetLongDescription()}\n</size>";

        // ��ְҵ
        foreach (var subRole in subRoles)
        {
            result += $"<color={subRole.Color.ToColorHexString()}><size=80%>{subRole.Name}</size>" +
                  $"<b><size=50%>({subRole.ShortDescription})</size></b></color>" +
                  $"<size=40%>:{subRole.GetLongDescription()}\n</size>";
        }

        return result;
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class HudManagerUpdatePatch
    {
        public static void Postfix()
        {
            if (!GameStates.InRealGame) return;

            if (Input.GetKeyDown(KeyCode.F1))
            {
                UpdateLocalPlayerRoleInfo();
                GameUtils.Popup?.Show(_localPlayerShower);
            }
        }
    }
}