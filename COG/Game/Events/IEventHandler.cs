using COG.Utils;

namespace COG.Game.Events;

public interface IEventHandler
{
    public GameEventType EventType { get; }
    public IGameEvent Handle(CustomPlayerData player, params object[] extraArguments);
}
