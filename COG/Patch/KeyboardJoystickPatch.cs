using COG.Game.CustomWinner;
using COG.Role;
using COG.Utils;
using System.Linq;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class KeyboardJoystickPatch
{
    public static void Postfix()
    {
        var data = CustomWinnerManager.GetManager().WinnableData;
        if (Input.GetKeyDown(KeyCode.Escape) && AmongUsClient.Instance.AmHost)
        {
            data.WinColor = Color.white;
            data.Winnable = true;
            data.WinnableCampType = CampType.Crewmate | CampType.Neutral | CampType.Impostor | CampType.Unknown;
            data.WinnablePlayers.AddRange(PlayerUtils.GetAllPlayers().Select(p => p.Data));
            data.WinText = "Force End";
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
        }
    }
}