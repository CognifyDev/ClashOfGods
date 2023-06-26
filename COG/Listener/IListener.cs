namespace COG.Listener;

/// <summary>
/// 监听器接口，用来监听游戏中的行为
/// </summary>
public interface IListener
{
    /// <summary>
    /// 当一个玩家被击杀的时候，触发该监听器
    /// </summary>
    /// <param name="killer">击杀玩家的玩家</param>
    /// <param name="target">被击杀的玩家</param>
    /// <returns>是否取消事件[false为取消]</returns>
    bool OnPlayerMurder(PlayerControl killer, PlayerControl target) { return true; }

    /// <summary>
    /// 当房主发送消息的时候，触发这个监听器
    /// </summary>
    /// <param name="controller">聊天控制器</param>
    /// <returns>是否取消事件[false为取消]</returns>
    bool OnHostChat(ChatController controller) { return true; }

    /// <summary>
    /// 当普通玩家发送消息的时候，触发这个监听器
    /// </summary>
    /// <param name="player">玩家</param>
    /// <param name="text">信息内容</param>
    /// <returns>是否取消事件[false为取消](?)</returns>
    bool OnPlayerChat(PlayerControl player, string text) { return true; }

    /// <summary>
    /// 聊天更新的时候触发
    /// </summary>
    /// <param name="controller">控制器</param>
    void OnChatUpdate(ChatController controller) { }

    /// <summary>
    /// 游戏CO开始的时候触发
    /// </summary>
    void OnCoBegin() { }

    /// <summary>
    /// 游戏结束的时候触发
    /// </summary>
    /// <param name="client">客户端</param>
    /// <param name="endGameResult">游戏结束结果</param>
    void OnGameEnd(AmongUsClient client, EndGameResult endGameResult) { }

    /// <summary>
    /// 游戏开始的时候触发
    /// </summary>
    /// <param name="manager"></param>
    void OnGameStart(GameStartManager manager) { }

    /// <summary>
    /// 游戏房间切换为公开的时候触发
    /// </summary>
    /// <param name="manager"></param>
    /// <returns></returns>
    bool OnMakePublic(GameStartManager manager) { return true; }
}