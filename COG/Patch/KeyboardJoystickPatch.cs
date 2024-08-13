using COG.Game.CustomWinner;
using COG.Role;
using COG.Utils;
using UnityEngine;

namespace COG.Patch;

[HarmonyPatch(typeof(KeyboardJoystick), nameof(KeyboardJoystick.Update))]
public static class KeyboardJoystickPatch
{
    public static void Postfix()
    {
        var data = CustomWinnerManager.GetManager().WinnableData;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            data.WinColor = Color.white;
            data.Winnable = true;
            data.WinnableCampType = CampType.Crewmate | CampType.Neutral | CampType.Impostor | CampType.Unknown;
            data.WinnablePlayers.AddRange(PlayerUtils.GetAllPlayers());
            data.WinText = "Force End";
            GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, false);
        }
    }
}