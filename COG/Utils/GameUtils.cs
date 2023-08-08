using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using COG.UI.CustomOption;

namespace COG.Utils;

public static class GameUtils
{
    public static Dictionary<PlayerControl, Role.Role> Data { get; internal set; } = new();

    /// <summary>
    ///     向游戏里面发送一条信息
    /// </summary>
    /// <param name="text">信息内容</param>
    public static void SendGameMessage(string text)
    {
        if (DestroyableSingleton<HudManager>._instance)
            DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(text);
    }


    public static Role.Role? GetLocalPlayerRole()
    {
        var player = PlayerControl.LocalPlayer;
        return Data.Where(keyValuePair => keyValuePair.Key.IsSamePlayer(player))
            .Select(keyValuePair => keyValuePair.Value).FirstOrDefault();
    }

    public static NormalGameOptionsV07 GetGameOptions()
    {
        return GameOptionsManager.Instance.currentNormalGameOptions;
    }
}