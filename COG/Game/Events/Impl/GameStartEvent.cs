namespace COG.Game.Events.Impl;

public class GameStartEvent : GameEventBase
{
    public GameStartEvent() : base(EventType.GameStart, null!)
    {
    }
}
