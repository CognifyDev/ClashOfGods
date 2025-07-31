namespace COG.Game.Events.Impl;

public class GameStartEvent : GameEventBase
{
    public GameStartEvent() : base(GameEventType.GameStart, null!)
    {
    }
}
