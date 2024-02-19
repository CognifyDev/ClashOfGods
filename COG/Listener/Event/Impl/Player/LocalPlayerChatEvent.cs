namespace COG.Listener.Event.Impl.Player;

public class LocalPlayerChatEvent : PlayerEvent
{
    private readonly ChatController _chatController;

    public LocalPlayerChatEvent(PlayerControl host, ChatController chatController) : base(host)
    {
        _chatController = chatController;
    }

    /// <summary>
    /// 获取聊天控制器
    /// </summary>
    /// <returns>控制器</returns>
    public ChatController GetChatController()
    {
        return _chatController;
    }
}