namespace COG.Listener.Event.Impl;

public class IntroCutsceneEvent : Listener.Event.Event
{
    public IntroCutscene IntroCutscene { get; }
    
    public IntroCutsceneEvent(IntroCutscene introCutscene)
    {
        IntroCutscene = introCutscene;
    }
}