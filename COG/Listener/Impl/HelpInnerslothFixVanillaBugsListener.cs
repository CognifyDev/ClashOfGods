using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Listener.Event.Impl.Player;
using COG.Listener.Event.Impl.RManager;
using COG.Utils;
using UnityEngine;

namespace COG.Listener.Impl;

public class VanillaBugFixListener : IListener
{
    public static bool OccuredBlackoutOnAirship { get; set; }

    [EventHandler(EventHandlerType.Postfix)]
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
        controller.StartCoroutine(CoFixBlackout(controller).WrapToIl2Cpp());
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnAirshipEjectionEnd(PlayerExileEndOnAirshipEvent @event)
    {
        OnEjectionEnd(@event.Controller);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnOtherMapEjectionEnd(PlayerExileEndEvent @event)
    {
        OnEjectionEnd(@event.ExileController);
    }

    public IEnumerator CoFixBlackout(ExileController controller)
    {
        Main.Logger.LogInfo("Checking if blackout occured...");

        yield return new WaitForSeconds(0.5f);

        var hud = HudManager.Instance;
        var fullScr = hud.FullScreen.gameObject;

        if (!fullScr.active) yield break;

        Main.Logger.LogWarning("After-meeting blackout bug has occured. Trying to fix...");
        var mapId = (MapNames)GameUtils.GetGameOptions().MapId;

        if (mapId is MapNames.Airship)
        {
            var ship = ShipStatus.Instance;
            yield return ship.PrespawnStep();
        }

        controller.ReEnableGameplay();
        Main.Logger.LogInfo("Fixed successfully!");
    }
}