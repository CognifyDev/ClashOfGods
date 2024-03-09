namespace COG.Listener.Event.Impl;

public class IntroCutsceneEvent : Event
{
    public IntroCutscene IntroCutscene { get; }

    public IntroCutsceneEvent(IntroCutscene introCutscene)
    {
        IntroCutscene = introCutscene;
    }
}