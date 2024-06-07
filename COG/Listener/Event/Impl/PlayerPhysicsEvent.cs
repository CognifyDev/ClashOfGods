namespace COG.Listener.Event.Impl;

public class PlayerPhysicsEvent : Event
{
    public PlayerPhysicsEvent(PlayerPhysics playerPhysics)
    {
        PlayerPhysics = playerPhysics;
    }

    public PlayerPhysics PlayerPhysics { get; }
}