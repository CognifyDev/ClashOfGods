namespace COG.NewListener.Event.Impl.PPhysics;

public class PlayerPhysicsCoSpawnEvent : PlayerPhysicsEvent
{
    public LobbyBehaviour LobbyBehaviour { get; }
    
    public PlayerPhysicsCoSpawnEvent(PlayerPhysics playerPhysics, LobbyBehaviour lobbyBehaviour) : base(playerPhysics)
    {
        LobbyBehaviour = lobbyBehaviour;
    }
}