using COG.Utils;

namespace COG.Game.Events.Impl;

public class FinishTaskGameEvent : GameEventBase
{
    public FinishTaskGameEvent(CustomPlayerData player, uint taskId) : base(GameEventType.FinishTask, player)
    {
        TaskId = taskId;
    }

    public uint TaskId { get; }
}