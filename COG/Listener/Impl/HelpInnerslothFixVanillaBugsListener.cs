using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Game.CustomWinner;
using COG.Listener.Event.Impl.Player;
using COG.Listener.Event.Impl.RManager;
using COG.Utils;
using UnityEngine;

namespace COG.Listener.Impl;

public class VanillaBugFixListener : IListener
{
    public void OnSelectRole(RoleManagerSelectRolesEvent _)
    {
        var lobby = LobbyBehaviour.Instance;
        if (lobby)
        {
            Main.Logger.LogWarning("Lobby room still exists in this room, trying to fix...");
            lobby.Despawn();
        }
    }

    public void OnEjectionEnd(ExileController controller)
    {
        controller.StartCoroutine(CoFixBlackout().WrapToIl2Cpp());
    }

    public void OnAirshipEjectionEnd(PlayerExileEndOnAirshipEvent @event) => OnEjectionEnd(@event.Controller);
    public void OnOtherMapEjectionEnd(PlayerExileEndEvent @event) => OnEjectionEnd(@event.ExileController);

    public IEnumerator CoFixBlackout()
    {
        yield return new WaitForSeconds(1f);

        var hud = HudManager.Instance;
        var fullScr = hud.FullScreen.gameObject;
        if (!fullScr.active) yield break;

        Main.Logger.LogInfo("After-meeting blackout bug has occured. Trying to fix...");
        var auClient = AmongUsClient.Instance;
        var mapId = (MapNames)(auClient.NetworkMode == NetworkModes.FreePlay ? auClient.TutorialMapId : GameUtils.GetGameOptions().MapId);
        
        if (mapId is MapNames.Airship)
        {
            Main.Logger.LogError("The game will end now because you're playing in Airship and we can't fix blackout bug in Airship.");
            CustomWinnerManager.EndGame(PlayerControl.AllPlayerControls.ToArray(), "Force End Due to Bug", Palette.White);
        }
        else
        {
            fullScr.SetActive(false);
            Camera.main.GetComponent<FollowerCamera>().Locked = false;
            Main.Logger.LogInfo("Fixed successfully!");
        }
    }
}