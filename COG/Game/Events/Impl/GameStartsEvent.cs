namespace COG.Game.Events.Impl;

public class GameStartsEvent : GameEventBase
{
    public GameStartsEvent() : base(GameEventType.GameStart, null)
    {
    }
}