namespace COG.Listener.Event.Impl;

public class PassiveButtonEvent : Event
{
    public PassiveButtonEvent(PassiveButton button)
    {
        Button = button;
    }
    
    public PassiveButton Button { get; }
}