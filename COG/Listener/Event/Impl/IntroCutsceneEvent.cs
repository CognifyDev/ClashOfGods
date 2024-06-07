namespace COG.Listener.Event.Impl;

public class IntroCutsceneEvent : Event
{
    public IntroCutsceneEvent(IntroCutscene introCutscene)
    {
        IntroCutscene = introCutscene;
    }

    public IntroCutscene IntroCutscene { get; }
}