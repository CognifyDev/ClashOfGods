using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Game.CustomWinner.Data;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using COG.Utils.Coding;
using InnerNet;
using UnityEngine;
using GameStates = COG.States.GameStates;

namespace COG.Role.Impl.Neutral;

[NotTested]
[WorkInProgress("未完成：会向其他人显示报告者是自己")]
public class Reporter : CustomRole, IListener, IWinnable
{
    private int _reportedTimes;
    private bool _finishedReporting;

    private PlayerControl? _winnablePlayer;
    
    public override void ClearRoleGameData()
    {
        _reportedTimes = 0;
        _finishedReporting = false;

        _winnablePlayer = null;
    }

    private readonly CustomOption _neededReportTimes;
    
    public Reporter() : base(new Color(158, 30, 26, 100), CampType.Neutral)
    {
        _neededReportTimes = CreateOption(() => LanguageConfig.Instance.NeededReportTimes,
            new IntOptionValueRule(1, 1, 14, 2));
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerReport(PlayerReportDeadBodyEvent @event)
    {
        var player = @event.Player;
        if (!player.IsRole(this) || !PlayerControl.LocalPlayer.IsSamePlayer(player)) return;
        _reportedTimes ++;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerFixedUpdate(PlayerFixedUpdateEvent _)
    {
        if (!_finishedReporting && _reportedTimes < _neededReportTimes.GetInt())
        {
            _finishedReporting = true;

            var rpc = RpcUtils.StartRpcImmediately(PlayerControl.LocalPlayer, KnownRpc.ReporterFinishReporting);
            rpc.WriteNetObject(PlayerControl.LocalPlayer);
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnHandleRpc(PlayerHandleRpcEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        if ((byte) KnownRpc.ReporterFinishReporting != @event.CallId) return;
        var reader = @event.Reader;
        var winner = reader.ReadNetObject<PlayerControl>();
        _winnablePlayer = winner;
    }

    public override IListener GetListener()
    {
        return this;
    }

    public void CheckWin(WinnableData data)
    {
        if (GameStates.IsMeeting) return;
        if (_winnablePlayer == null) return;
        
        data.WinnableCampType = CampType; 
        data.WinText = LanguageConfig.Instance.NeutralsWinText.CustomFormat(_winnablePlayer);
        data.WinColor = Color;
        data.WinnablePlayers.Add(_winnablePlayer.Data);
        data.Winnable = true;
    }

    public uint GetWeight()
    {
        return IWinnable.GetOrder(6);
    }
}