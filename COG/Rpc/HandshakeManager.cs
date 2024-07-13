using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.Listener.Impl;
using COG.Utils;
using COG.Utils.Version;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace COG.Rpc;

public class HandshakeManager
{
    private static HandshakeManager? _instance;
    public static HandshakeManager Instance => _instance ??= new();
    public Dictionary<PlayerControl, (VersionInfo, DateTime)> PlayerVersionInfo { get; } = new();
    
    public HandshakeResultFlag AddInfo(PlayerControl pc, string verStr, string timeStr)
    {
        VersionInfo version = null!;
        try
        {
            version = VersionInfo.Parse(verStr);
        }
        catch
        {
            return HandshakeResultFlag.InvalidVersion;
        }

        if (!DateTime.TryParse(timeStr, out var time)) return HandshakeResultFlag.InvalidCommitTime;
        if (!(pc && PlayerVersionInfo.TryAdd(pc, (version, time)))) return HandshakeResultFlag.InvalidPlayer;

        return HandshakeResultFlag.Success;
    }

    public void RpcHandshake()
    {
        RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.Handshake)
                .Write(Main.VersionInfo.ToString()).Write(GitInfo.CommitDate).Finish();
    }

    public (VersionInfo, DateTime) GetHostInfo() => PlayerVersionInfo.GetValueOrDefault(AmongUsClient.Instance.GetHost().Character);

    public string CheckModdedClientVersions()
    {
        var hostInfo = GetHostInfo();
        var version = hostInfo.Item1;
        var dateHost = hostInfo.Item2;
        var invalidMsgBuilder = new StringBuilder();
        if (!DateTime.TryParse(GitInfo.CommitDate, out var myDate)) return "";

        if (AmongUsClient.Instance.AmHost)
        {
            foreach (var (player, (pVersion, time)) in PlayerVersionInfo)
            {
                if (pVersion.Equals(Main.VersionInfo))
                {
                    if (myDate.CompareTo(time) != 0)
                    {
                        invalidMsgBuilder.Append("InvalidModCommitDateHost %player% %version% (at %commitDate%)\n".CustomFormat(player.Data.PlayerName, pVersion, time));
                        Main.Logger.LogWarning($"Invalid modded client version: {player} ({player.PlayerId}) {pVersion} {time}");
                    }
                }
                else
                {
                    invalidMsgBuilder.Append("InvalidModVersionHost %player% %version%\n".CustomFormat(player.Data.PlayerName, pVersion));
                    Main.Logger.LogWarning($"Uncompatible client version: {player} ({player.PlayerId}) {pVersion}");
                }
            }
        }
        else // Just check self
        {
            if (version.Equals(Main.VersionInfo))
            {
                if (myDate.CompareTo(dateHost) != 0)
                {
                    invalidMsgBuilder.Append("InvalidModCommitDateClient %version% (at %commitDate%)\n".CustomFormat(version.ToString(), dateHost.ToString()));
                    Main.Logger.LogWarning($"Invalid modded host commit date: {version} {dateHost}");
                }
            }
            else
            {
                invalidMsgBuilder.Append("InvalidModVersionClient %version%\n".CustomFormat(version.ToString()));
                Main.Logger.LogWarning($"Uncompatible modded host version: {version}");
            }
        }

        return invalidMsgBuilder.ToString();
    }

    public string CheckUnmoddedClients()
    {
        var allPlayers = new List<PlayerControl>(PlayerControl.AllPlayerControls.ToArray());
        var unmodded = allPlayers.Concat(PlayerVersionInfo.Keys).Distinct();
        var unmoddedBuilder = new StringBuilder();

        unmodded.ForEach(p => unmoddedBuilder.Append("UnmoddedPlayerMessage %player%\n".CustomFormat(p.Data.PlayerName)));
        return unmoddedBuilder.ToString();
    }

    public const string ShowerObjectName = "COGErrorMsgShower";
    public bool HasCreatedShower => HudManager.Instance.transform.Find(ShowerObjectName);
    public TextMeshPro? TextShower { get; private set; }

    public void CheckPlayersAndDisplay()
    {
        var fullString = CheckUnmoddedClients() + CheckModdedClientVersions();
        var template = HudManager.Instance.transform.Find(VersionShowerListener.ShowerObjectName);
        if (!template) return;

        if (!HasCreatedShower)
        {
            var shower = Object.Instantiate(template, template.parent);
            shower.localPosition = Vector3.zero;
            TextShower = shower.GetComponent<TextMeshPro>();
            TextShower.color = Palette.ImpostorRed;
            TextShower.alignment = TextAlignmentOptions.Center;
            TextShower.fontSize *= 2;
            TextShower.fontSizeMin = TextShower.fontSizeMax = TextShower.fontSize;
        }

        TextShower!.text = fullString;
    }

    public static IListener GetListener() => new Listener();

    public enum HandshakeResultFlag
    {
        Success = 0,
        InvalidVersion = 1,
        InvalidCommitTime = 2,
        InvalidPlayer = 3
    }

    private class Listener : IListener
    {
        public void OnJoinLobby(PlayerControlAwakeEvent @event) => @event.Player.StartCoroutine(CoCheckHandshake().WrapToIl2Cpp());
        
        private IEnumerator CoCheckHandshake()
        {
            yield return new WaitForSeconds(0.5f + ((float)PlayerControl.LocalPlayer.PlayerId / 10)); // Wait for initializing
            Instance.RpcHandshake();
            yield return new WaitForSeconds(0.25f);
            Instance.CheckPlayersAndDisplay();
        }
    }
}