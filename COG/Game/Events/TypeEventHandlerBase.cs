using COG.Utils;

namespace COG.Game.Events;

public abstract class TypeEventHandlerBase : IEventHandler
{
    public EventType EventType { get; }

    protected TypeEventHandlerBase(EventType @event)
    {
        EventType = @event;
    }

    IGameEvent IEventHandler.Handle(CustomPlayerData player, params object[] extraArguments)
    {
        return Handle(player, extraArguments);
    }

    public abstract IGameEvent Handle(CustomPlayerData player, params object[] extraArguments);
}
