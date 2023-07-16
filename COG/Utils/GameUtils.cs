using System.Collections.Generic;
using AmongUs.GameOptions;

namespace COG.Utils;

public class GameUtils
{
    public static readonly Dictionary<PlayerControl, Role.Role> Data = new();

    /// <summary>
    /// 向游戏里面发送一条信息
    /// </summary>
    /// <param name="text">信息内容</param>
    public static void SendGameMessage(string text)
    {
        if (DestroyableSingleton<HudManager>._instance) 
            DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(text);
    }

    public static NormalGameOptionsV07 GetGameOptions() => GameOptionsManager.Instance.currentNormalGameOptions;
}