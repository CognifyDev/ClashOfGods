using COG.Utils;

namespace COG.Game.Events;

public abstract class TypeEventHandlerBase : IEventHandler
{
    protected TypeEventHandlerBase(GameEventType @event)
    {
        EventType = @event;
    }

    public GameEventType EventType { get; }

    IGameEvent? IEventHandler.Handle(CustomPlayerData player, params object[] extraArguments)
    {
        return Handle(player, extraArguments);
    }

    public abstract IGameEvent? Handle(CustomPlayerData player, params object[] extraArguments);
}