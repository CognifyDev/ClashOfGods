namespace COG.NewListener.Event.Impl.GSManager;

public class GameStartManagerMakePublicEvent : GameStartManagerEvent
{
    public GameStartManagerMakePublicEvent(GameStartManager manager) : base(manager)
    {
    }
}