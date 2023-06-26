namespace COG.Utils;

public class GameUtils
{
    /// <summary>
    /// 向游戏里面发送一条信息
    /// </summary>
    /// <param name="text">信息内容</param>
    public static void SendGameMessage(string text)
    {
        if (DestroyableSingleton<HudManager>._instance) 
            DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(text);
    }
}