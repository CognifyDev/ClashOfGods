using COG.Utils;

namespace COG.Game.Events;

public interface IEventHandler
{
    public EventType EventType { get; }
    public IGameEvent Handle(CustomPlayerData player, params object[] extraArguments);
}
