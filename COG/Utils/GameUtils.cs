using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Config.Impl;
using COG.Constant;
using COG.Role;
using COG.Rpc;
using UnityEngine;

namespace COG.Utils;

public static class GameUtils
{
    public const MurderResultFlags DefaultFlag = MurderResultFlags.Succeeded | MurderResultFlags.DecisionByHost;
    public static List<CustomPlayerData> PlayerData { get; } = new();

    internal static GenericPopup? PopupPrefab { get; set; }
    public static GenericPopup? Popup
    { 
        get
        {
            if (!PopupPrefab) return null;
            var popup = Object.Instantiate(PopupPrefab, Camera.main!.transform, true)!;
            popup.transform.localPosition = new Vector3(0, 0, 0);
            return popup;
        }
    }

    /// <summary>
    ///     向游戏里面发送一条信息
    /// </summary>
    /// <param name="text">信息内容</param>
    public static void SendGameMessage(string text)
    {
        if (DestroyableSingleton<HudManager>.Instance is { } hud)
            hud.Notifier.AddDisconnectMessage(text);
    }

    /// <summary>
    /// 获取当前启用的内鬼数量
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static int GetImpostorsNumber()
    {
        var players = PlayerUtils.GetAllPlayers();
        if (players == null) throw new NullReferenceException("Player list is null");

        var playersNum = players.Count;
        int impostorsNum;

        switch (playersNum)
        {
            case <= 6:
            {
                impostorsNum = 1;
                break;
            }
            case <= 8:
            {
                impostorsNum = 2;
                break;
            }
            default:
            {
                impostorsNum = 3;
                break;
            }
        }

        var numberInOptions = GetGameOptions().NumImpostors;
        return impostorsNum >= numberInOptions ? numberInOptions : impostorsNum;
    }

    public static int GetNeutralNumber()
    {
        var players = PlayerUtils.GetAllPlayers();
        var count = players.Count - GetImpostorsNumber();
        var countInConfig = GlobalCustomOptionConstant.MaxNeutralNumber.GetInt();
        var finalCount = CustomRoleManager.GetManager().GetRoles()
            .Count(role => role.CampType == CampType.Neutral);
        if (finalCount < countInConfig) countInConfig = finalCount; 
        var result = count >= countInConfig ? countInConfig : count;
        return result < 0 ? 0 : result;
    }

    public static CustomRole GetLocalPlayerRole()
    {
        return PlayerControl.LocalPlayer.GetMainRole();
    }

    public static NormalGameOptionsV09 GetGameOptions()
    {
        return GameOptionsManager.Instance.currentNormalGameOptions;
    }

    public static List<Transform> GetAllChildren(this Transform transform)
    {
        List<Transform> result = new();
        for (var i = 0; i < transform.childCount; i++)
            result.Add(transform.GetChild(i));
        return result;
    }

    public static void SendSystemMessage(string text, float delay = 0.2f)
    {
        HudManager.Instance.Chat.StartCoroutine(CoSendChatMessage().WrapToIl2Cpp());
        return;

        IEnumerator CoSendChatMessage()
        {
            yield return new WaitForSeconds(delay);
            var host = AmongUsClient.Instance.GetHost().Character;
            var tempName = host.Data.PlayerName;
            host.SetName(LanguageConfig.Instance.SystemMessage);
            HudManager.Instance.Chat.AddChat(host, text, false);
            host.SetName(tempName);
        }
    }

    public static void RpcNotifySettingChange(int id, string text)
    {
        // 先给自己发通知
        HudManager.Instance.Notifier.SettingsChangeMessageLogic((StringNames)id, text, true);

        var writer = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.NotifySettingChange);
        writer.WritePacked(id).Write(text).Finish();
    }
}