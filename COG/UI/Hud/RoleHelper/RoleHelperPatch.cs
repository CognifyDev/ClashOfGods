using System.Collections.Generic;
using System.Linq;
using COG.Listener;
using COG.Listener.Attribute;
using COG.Listener.Event.Impl.Game;
using COG.Role;
using COG.Utils;
using HarmonyLib;
using UnityEngine;

namespace COG.UI.Hud.RoleHelper;

public static class RoleHelperManager
{
    private static Dictionary<byte, string> playerRoleDescriptions = new();
    private static string localPlayerShower = "";

    public static void UpdateLocalPlayerRoleInfo()
    {
        if (PlayerControl.LocalPlayer == null) return;

        var player = PlayerControl.LocalPlayer;
        var mainRole = player.GetMainRole();
        var subRoles = player.GetSubRoles().ToList();

        if (mainRole == null) return;

        localPlayerShower = BuildRoleDescription(mainRole, subRoles);

        playerRoleDescriptions[player.PlayerId] = localPlayerShower;
    }

    private static string BuildRoleDescription(CustomRole mainRole, List<CustomRole> subRoles)
    {
        var result = "";

        // 主职业
        result += $"<color={mainRole.Color.ToColorHexString()}><size=80%>{mainRole.Name}</size>" +
              $"<b><size=50%>({mainRole.ShortDescription})</size></b></color>" +
              $"<size=40%>:{mainRole.GetLongDescription()}\n</size>";

        // 副职业
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
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!GameStates.InRealGame) return;

            if (UnityEngine.Input.GetKeyDown(KeyCode.F1))
            {
                UpdateLocalPlayerRoleInfo();
                GameUtils.Popup?.Show(localPlayerShower);
            }
        }
    }
}