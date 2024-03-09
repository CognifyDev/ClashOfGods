namespace COG.Listener.Event.Impl;

public class GameStartManagerEvent : Event
{
    public GameStartManager GameStartManager { get; }

    public GameStartManagerEvent(GameStartManager manager)
    {
        GameStartManager = manager;
    }
}