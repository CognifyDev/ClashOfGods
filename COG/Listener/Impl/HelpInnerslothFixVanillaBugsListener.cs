using COG.Listener.Event.Impl.Player;
using COG.Listener.Event.Impl.RManager;

namespace COG.Listener.Impl;

/*
 * 这个类尝试修复树懒留下的逆天BUG
 */
public class VanillaBugFixListener : IListener
{
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
        controller.ReEnableGameplay();
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
}