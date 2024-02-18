namespace COG.NewListener.Event.Impl;

public class GameStartManagerEvent : Event
{
    public GameStartManager GameStartManager;
    
    public GameStartManagerEvent(GameStartManager manager)
    {
        GameStartManager = manager;
    }
}