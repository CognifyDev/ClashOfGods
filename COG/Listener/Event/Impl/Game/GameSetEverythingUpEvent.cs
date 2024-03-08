namespace COG.Listener.Event.Impl.Game;

public class GameSetEverythingUpEvent : GameEvent<EndGameManager>
{
    public GameSetEverythingUpEvent(EndGameManager endGameManager) : base(endGameManager)
    {
    }
}