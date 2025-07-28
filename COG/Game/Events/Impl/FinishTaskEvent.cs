using COG.Utils;

namespace COG.Game.Events.Impl;

public class FinishTaskEvent : GameEventBase
{
    public uint TaskId { get; } 

    public FinishTaskEvent(CustomPlayerData player, uint taskId) : base(EventType.FinishTask, player)
    {
        TaskId = taskId;
    }
}
