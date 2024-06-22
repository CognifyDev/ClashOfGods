using Il2CppSystem.Collections.Generic;

namespace COG.Listener.Event.Impl.Player;

/// <summary>
///     一个玩家被设置任务列表时，此事件会执行
/// </summary>
public class PlayerCoSetTasksEvent : PlayerEvent
{
    public PlayerCoSetTasksEvent(PlayerControl player, List<NetworkedPlayerInfo> tasks) : base(player)
    {
        TasksList = tasks;
    }

    /// <summary>
    ///     任务列表
    /// </summary>
    public List<NetworkedPlayerInfo> TasksList { get; set; }
}