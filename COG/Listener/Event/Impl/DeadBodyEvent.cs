namespace COG.Listener.Event.Impl;

public class DeadBodyEvent : Listener.Event.Event
{
    public DeadBody DeadBody { get; }
    
    public DeadBodyEvent(DeadBody deadBody)
    {
        DeadBody = deadBody;
    }
}