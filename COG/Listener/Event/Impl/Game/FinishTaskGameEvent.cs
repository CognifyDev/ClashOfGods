using COG.Game.Events;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game;

public class FinishTaskGameEvent : GameEventBase
{
    public FinishTaskGameEvent(CustomPlayerData player, uint taskId) : base(GameEventType.FinishTask, player)
    {
        TaskId = taskId;
    }

    public uint TaskId { get; }
}