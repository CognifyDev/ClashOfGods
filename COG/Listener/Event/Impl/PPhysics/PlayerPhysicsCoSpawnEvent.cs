namespace COG.Listener.Event.Impl.PPhysics;

public class PlayerPhysicsCoSpawnEvent : PlayerPhysicsEvent
{
    public PlayerPhysicsCoSpawnEvent(PlayerPhysics playerPhysics, LobbyBehaviour lobbyBehaviour) : base(playerPhysics)
    {
        LobbyBehaviour = lobbyBehaviour;
    }

    public LobbyBehaviour LobbyBehaviour { get; }
}