namespace COG.Listener.Event.Impl;

public class DeadBodyEvent : Event
{
    public DeadBody DeadBody { get; }

    public DeadBodyEvent(DeadBody deadBody)
    {
        DeadBody = deadBody;
    }
}