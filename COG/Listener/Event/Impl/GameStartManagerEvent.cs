namespace COG.Listener.Event.Impl;

public class GameStartManagerEvent : Listener.Event.Event
{
    public GameStartManager GameStartManager;
    
    public GameStartManagerEvent(GameStartManager manager)
    {
        GameStartManager = manager;
    }
}