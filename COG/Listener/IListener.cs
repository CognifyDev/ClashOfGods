using Il2CppSystem.Collections;
using Il2CppSystem.Collections.Generic;
using InnerNet;

namespace COG.Listener;

/// <summary>
///     监听器接口，用来监听游戏中的行为
/// </summary>
public interface IListener
{
    public static readonly IListener Empty = new EmptyListener();

    /// <summary>
    ///     当一个玩家被击杀的时候，触发该监听器
    /// </summary>
    /// <param name="killer">击杀玩家的玩家</param>
    /// <param name="target">被击杀的玩家</param>
    /// <returns>是否取消事件[false为取消]</returns>
    bool OnPlayerMurder(PlayerControl killer, PlayerControl target)
    {
        return true;
    }

    /// <summary>
    ///     当房主发送消息的时候，触发这个监听器
    /// </summary>
    /// <param name="controller">聊天控制器</param>
    /// <returns>是否取消事件[false为取消]</returns>
    bool OnHostChat(ChatController controller)
    {
        return true;
    }

    /// <summary>
    ///     当普通玩家发送消息的时候，触发这个监听器
    /// </summary>
    /// <param name="player">玩家</param>
    /// <param name="text">信息内容</param>
    /// <returns>是否取消事件[false为取消](?)</returns>
    bool OnPlayerChat(PlayerControl player, string text)
    {
        return true;
    }

    /// <summary>
    ///     聊天更新的时候触发
    /// </summary>
    /// <param name="controller">控制器</param>
    void OnChatUpdate(ChatController controller)
    {
    }

    /// <summary>
    ///     游戏开始显示身份的时候触发
    /// </summary>
    void OnCoBegin()
    {
    }

    /// <summary>
    ///     游戏结束的时候触发
    /// </summary>
    /// <param name="client">客户端</param>
    /// <param name="endGameResult">游戏结束结果</param>
    void AfterGameEnd(AmongUsClient client, ref EndGameResult endGameResult)
    {
    }

    /// <summary>
    ///     游戏开始倒计时结束后触发
    /// </summary>
    /// <param name="manager"></param>
    void OnGameStart(GameStartManager manager)
    {
    }

    /// <summary>
    ///     游戏真正开始（玩家可移动）时触发
    /// </summary>
    /// <param name="manager"></param>
    void OnGameStart(GameManager manager)
    {
    }

    /// <summary>
    ///     游戏房间切换为公开的时候触发
    /// </summary>
    /// <param name="manager"></param>
    /// <returns></returns>
    bool OnMakePublic(GameStartManager manager)
    {
        return true;
    }

    void OnPingTrackerUpdate(PingTracker tracker)
    {
    }

    bool OnSetUpRoleText(IntroCutscene intro,
        ref IEnumerator roles)
    {
        return true;
    }

    void OnSetUpTeamText(IntroCutscene intro,
        ref List<PlayerControl> teamToDisplay)
    {
    }

    void AfterSetUpTeamText(IntroCutscene intro)
    {
    }

    void OnPlayerExile(ExileController controller)
    {
    }

    void OnAirshipPlayerExile(AirshipExileController controller)
    {
    }

    void OnPlayerLeft(AmongUsClient client, ClientData data, DisconnectReasons reason)
    {
    }

    void OnPlayerJoin(AmongUsClient client, ClientData data)
    {
    }

    void OnSelectRoles()
    {
    }

    void OnGameJoined(AmongUsClient amongUsClient, string gameCode)
    {
    }

    void OnGameEndSetEverythingUp(EndGameManager manager)
    {
    }

    void OnIGameOptionsExtensionsDisplay(ref string result)
    {
    }

    void OnKeyboardJoystickUpdate(KeyboardJoystick keyboardJoystick)
    {
    }

    void OnRPCReceived(byte callId, MessageReader reader)
    {
    }

    bool OnPlayerReportDeadBody(PlayerControl playerControl, GameData.PlayerInfo? target)
    {
        return true;
    }

    void OnMurderPlayer(PlayerControl killer, PlayerControl target)
    {
    }

    void OnSettingInit(OptionsMenuBehaviour menu)
    {
    }

    void OnHudStart(HudManager? hud)
    {
    }

    void OnHudUpdate(HudManager manager)
    {
    }

    void AfterPlayerFixedUpdate(PlayerControl player)
    {
    }

    void OnGameEnd(AmongUsClient client, ref EndGameResult endGameResult)
    {
    }

    void OnKeyboardPass()
    {
    }

    bool OnPlayerVent(Vent vent, GameData.PlayerInfo playerInfo, ref bool canUse, ref bool couldUse, ref float cooldown)
    {
        return true;
    }

    bool OnCheckGameEnd()
    {
        return true;
    }

    bool OnCheckTaskCompletion(ref bool allow)
    {
        return true;
    }

    bool OnShowSabotageMap(MapBehaviour mapBehaviour)
    {
        return true;
    }

    bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        return true;
    }

    void AfterRPCReceived(byte callId, MessageReader reader)
    {
    }

    bool OnDeadBodyClick(DeadBody deadBody)
    {
        return true;
    }

    void OnCoSetTasks(PlayerControl player, List<GameData.TaskInfo> tasks)
    {
    }

    void OnGameStartManagerUpdate(GameStartManager manager)
    {
    }

    void OnGameStartCountdownEnd(GameStartManager manager)
    {
    }

    private class EmptyListener : IListener
    {
    }
}