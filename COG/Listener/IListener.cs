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
    void OnGameStartWithMovement(GameManager manager)
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

    void OnLobbyRPCReceived(byte callId, MessageReader reader)
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

    /// <summary>
    ///     当一个玩家向房主发送变形请求的时候，触发该监听器
    /// </summary>
    /// <param name="player">发出变形请求的玩家</param>
    /// <param name="target">变形的目标</param>
    /// <param name="shouldAnimate">是否显示变形动画</param>
    /// <returns>是否取消事件[false为取消]</returns>
    ///     如果通过return false取消本次请求，应该向player发出RejectShapeshift
    bool OnCheckShapeshift(PlayerControl player, PlayerControl target, bool shouldAnimate)
    {
        return true;
    }

    /// <summary>
    ///     当变形请求被执行时，触发该监听器
    /// </summary>
    /// <param name="player">变形者</param>
    /// <param name="target">变形的目标</param>
    /// <param name="shouldAnimate">是否显示变形动画</param>
    void OnShapeshift(PlayerControl player, PlayerControl target, bool shouldAnimate)
    {
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

    void OnHudDestroy(HudManager hud)
    {
    }


    void OnCoSpawnPlayer(PlayerPhysics playerPhysics, LobbyBehaviour lobbyBehaviour)
    {
    }

    void OnCreatePlayer(AmongUsClient amongUsClient, ClientData client)
    {
    }

    void OnTaskAdderShowFolder(TaskAdderGame taskAdderGame, TaskFolder folder)
    {
    }

    void AfterTaskAdderShowFolder(TaskAdderGame taskAdderGame, TaskFolder folder)
    {
    }

    void OnTaskButtonUpdate(TaskAddButton button)
    {
    }

    bool OnTaskButtonAddTask(TaskAddButton button)
    {
        return true;
    }

    void OnMeetingStart(MeetingHud meetingHud)
    {
    }

    /// <summary>
    /// 当一个玩家向房主发送投票请求时触发此监听器
    /// return false会撤销这个投票请求，并且会触发RpcClearVote清除玩家方的已投票状态
    /// </summary>
    /// <param name="meetingHud"></param>
    /// <param name="voter">投票玩家的PlayerControl</param>
    /// <param name="target">被投目标的PlayerControl</param>
    /// <returns></returns>
    bool OnCastVote(MeetingHud meetingHud, PlayerControl voter, PlayerControl target)
    {
        return true;
    }

    /// <summary>
    /// 当房主确认并执行了玩家的投票时触发该监听器
    /// </summary>
    /// <param name="meetingHud"></param>
    /// <param name="voter">投票玩家的PlayerControl</param>
    /// <param name="target">被投目标的PlayerControl</param>
    void OnVoted(MeetingHud meetingHud, PlayerControl voter, PlayerControl target)
    {
    }

    /// <summary>
    /// 这是meetingHud发生更新时的实时监听器
    /// Update函数每秒钟执行30次，所以这个监听器每秒也是30次
    /// 大部分对meetinghud的监听应该由OnMeetingHudLateUpdate处理
    /// 只有实时且必要的监听器应该使用该listener
    /// </summary>
    /// <param name="meetingHud"></param>
    void OnMeetingHudUpdate(MeetingHud meetingHud)
    {
    }
    /// <summary>
    /// 这是meetingHud发生更新时的延时监听器
    /// Update函数每秒钟执行30次，而这个监听器每10次运行一次，以防止卡顿
    /// 如果这造成了卡顿，应该考虑调整buffertime为更大值，10次是参考的TOH的处理
    /// 应用此监听器进行会议上技能按钮的更新和结束会议的行为
    /// </summary>
    /// <param name="meetingHud"></param>
    void OnMeetingHudLateUpdate(MeetingHud meetingHud)
    {
    }

    /// <summary>
    /// 当会议讨论终止，向玩家展示投票结果动画开始时触发此监听器
    /// 应用此监听器移除会议技能按钮
    /// </summary>
    /// <param name="meetingHud"></param>
    void OnVoteResultsShown(MeetingHud meetingHud)
    {
    }

    /// <summary>
    /// 就在播放驱逐动画前那么一小会，运行这个监听器
    /// 这个监听器的主要作用估计就是修改被驱逐玩家的名字，来展示自定义的驱逐文字
    /// 不对，非H系模组可以直接patch ExileController里面显示的文字，我是笨蛋
    /// </summary>
    /// <param name="meetingHud"></param>
    void BeforeExileThePlayer(MeetingHud meetingHud, GameData.PlayerInfo exiled, bool tie)
    {
    }

    /// <summary>
    /// 当MeetingHud被摧毁，也就是会议完全结束回到正常游戏时，运行此监听器
    /// </summary>
    /// <param name="meetingHud"></param>
    void OnMeetingFinished(MeetingHud meetingHud)
    {
    }

    private class EmptyListener : IListener { }
}