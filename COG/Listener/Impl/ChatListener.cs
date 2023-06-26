namespace COG.Listener.Impl;

public class ChatListener : IListener
{
    public void OnChatUpdate(ChatController controller)
    {
        if (controller.TextArea.text == null || controller.TextArea.text.Equals(""))
        {
            return;
        }

        // 禁用聊天冷却
        DestroyableSingleton<HudManager>.Instance.Chat.AddChat(PlayerControl.LocalPlayer, controller.TextArea.text);
        controller.TimeSinceLastMessage = 0f; // 防止被树懒反作弊误判
    }
}