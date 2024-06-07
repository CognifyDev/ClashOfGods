namespace COG.Listener.Event.Impl;

public class GameStartManagerEvent : Event
{
    public GameStartManagerEvent(GameStartManager manager)
    {
        GameStartManager = manager;
    }

    public GameStartManager GameStartManager { get; }
}