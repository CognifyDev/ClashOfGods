namespace COG.Listener.Event.Impl.Game;

public class GameStartEvent : GameEvent<GameManager>
{
    public GameStartEvent(GameManager obj) : base(obj)
    {
    }
}