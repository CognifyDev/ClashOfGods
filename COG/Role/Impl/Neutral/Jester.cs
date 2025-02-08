using System.Linq;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Game.CustomWinner.Data;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.States;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Neutral;

public class Jester : CustomRole, IListener, IWinnable
{
    private readonly CustomOption _allowReportDeadBody;
    private readonly CustomOption _allowStartMeeting;

    public Jester() : base(Color.magenta, CampType.Neutral)
    {
        _allowStartMeeting = CreateOption(() => LanguageConfig.Instance.AllowStartMeeting,
            new BoolOptionValueRule(true));
        _allowReportDeadBody = CreateOption(() => LanguageConfig.Instance.AllowReportDeadBody,
            new BoolOptionValueRule(true));
    }

    public void CheckWin(WinnableData data)
    {
        var jesters = DeadPlayer.DeadPlayers.Where(dp =>
            dp.Data.IsRole(this) && dp.DeathReason == CustomDeathReason.Exiled); // Only one jester can win so only one will die due to kill

        var deadPlayers = jesters.ToArray();
        if (!deadPlayers.Any()) return;

        data.WinnableCampType = CampType; 
        data.WinText = LanguageConfig.Instance.NeutralsWinText.CustomFormat(deadPlayers[0].Data.PlayerName);
        data.WinColor = Color;
        data.WinnablePlayers.Add(deadPlayers[0].Data);
        data.Winnable = true;
    }

    public uint GetWeight()
    {
        return IWinnable.GetOrder(5);
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnCheckStartMeeting(PlayerReportDeadBodyEvent @event)
    {
        if (!GameStates.InGame || GameStates.InLobby) return true;
        var victim = @event.Target;
        var player = @event.Player;
        if (!player.IsRole(this)) return true;

        var allowMeeting = _allowStartMeeting.GetBool();
        var allowReport = _allowReportDeadBody.GetBool();

        if (!allowMeeting && !victim)
            return false; // Reject if meeting is unallowed (It's a meeting when victim is null)
        if (!allowReport && victim) return false; // Reject if reporting is unallowed
        return true;
    }

    public override IListener GetListener()
    {
        return this;
    }
}