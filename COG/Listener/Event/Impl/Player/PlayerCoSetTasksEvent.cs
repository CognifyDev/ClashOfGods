using Il2CppSystem.Collections.Generic;

namespace COG.Listener.Event.Impl.Player;

public class PlayerCoSetTasksEvent : PlayerEvent
{
    public List<GameData.TaskInfo> TasksList { get; }

    public PlayerCoSetTasksEvent(PlayerControl player, List<GameData.TaskInfo> tasks) : base(player)
    {
        TasksList = tasks;
    }
}