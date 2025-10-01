using COG.Game.Events;

namespace COG.Listener.Event.Impl.Game.Record;

public class StartGameEvent : GameEventBase
{
    public StartGameEvent() : base(GameEventType.GameStart, null)
    {
    }
}