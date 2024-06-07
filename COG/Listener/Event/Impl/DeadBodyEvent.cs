namespace COG.Listener.Event.Impl;

public class DeadBodyEvent : Event
{
    public DeadBodyEvent(DeadBody deadBody)
    {
        DeadBody = deadBody;
    }

    public DeadBody DeadBody { get; }
}