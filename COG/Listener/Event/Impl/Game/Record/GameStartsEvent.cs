using COG.Game.Events;

namespace COG.Listener.Event.Impl.Game.Record;

public class GameStartsEvent : GameEventBase
{
    public GameStartsEvent() : base(GameEventType.GameStart, null)
    {
    }
}