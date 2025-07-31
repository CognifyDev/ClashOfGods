using COG.Utils;

namespace COG.Game.Events;

public abstract class TypeEventHandlerBase : IEventHandler
{
    public GameEventType EventType { get; }

    protected TypeEventHandlerBase(GameEventType @event)
    {
        EventType = @event;
    }

    IGameEvent IEventHandler.Handle(CustomPlayerData player, params object[] extraArguments)
    {
        return Handle(player, extraArguments);
    }

    public abstract IGameEvent Handle(CustomPlayerData player, params object[] extraArguments);
}
