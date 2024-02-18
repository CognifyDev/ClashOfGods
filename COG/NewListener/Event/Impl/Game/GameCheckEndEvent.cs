using COG.Listener.Event.Impl;

namespace COG.NewListener.Event.Impl.Game;

public class GameCheckEndEvent : GameEvent<LogicGameFlowNormal>
{
    public GameCheckEndEvent(LogicGameFlowNormal obj) : base(obj)
    {
    }
}