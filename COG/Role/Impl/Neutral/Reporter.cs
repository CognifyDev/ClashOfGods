using System.Collections.Generic;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Game.CustomWinner.Data;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;
using GameStates = COG.States.GameStates;
using Random = System.Random;

namespace COG.Role.Impl.Neutral;

public class Reporter : CustomRole, IListener, IWinnable
{
    private readonly Dictionary<PlayerControl, uint> _reportersWhoReported = new();
    
    public override void ClearRoleGameData()
    {
        _reportersWhoReported.Clear();
    }

    private readonly CustomOption _neededReportTimes;
    
    public Reporter() : base(Color.gray, CampType.Neutral)
    {
        _neededReportTimes = CreateOption(() => LanguageConfig.Instance.NeededReportTimes,
            new FloatOptionValueRule(1F, 1F, 14F, 3F));
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnPlayerReport(PlayerReportDeadBodyEvent @event)
    {
        var player = @event.Player;
        var target = @event.Target;
        if (!player.IsRole(this)) return true;

        if (target == null) return false;
        
        var allAlivePlayers = PlayerUtils.GetAllAlivePlayers();
        var randomPlayer = allAlivePlayers[new Random().Next(0, allAlivePlayers.Count - 1)];
        randomPlayer.ReportDeadBody(target);
        
        if (_reportersWhoReported.TryGetValue(player, out var times))
        {
            _reportersWhoReported[player] = ++times;
        }
        else
        {
            _reportersWhoReported.Add(player, 1);
        }
        return false;
    }

    public override IListener GetListener()
    {
        return this;
    }

    public void CheckWin(WinnableData data)
    {
        if (GameStates.IsMeeting) return;
        
        foreach (var (target, times) in _reportersWhoReported)
        {
            if (times < _neededReportTimes.GetFloat()) return;
            
            data.WinnableCampType = CampType; 
            data.WinText = LanguageConfig.Instance.NeutralsWinText.CustomFormat(target);
            data.WinColor = Color;
            data.WinnablePlayers.Add(target.Data);
            data.Winnable = true;
        }
    }

    public uint GetWeight()
    {
        return IWinnable.GetOrder(6);
    }
}