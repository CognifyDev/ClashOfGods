using System.Linq;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Game.CustomWinner.Data;
using COG.Game.Events;
using COG.Game.Events.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
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
        var ejectionEvents = EventRecorder.Instance.GetEvents().Where(e => e is PlayerExileGameEvent);
        var matchedEvent =
            ejectionEvents.FirstOrDefault(e => e.Player!.IsRole(this)); // select last exiled jester as winner

        if (matchedEvent == null) return;

        var winner = matchedEvent.Player;

        data.WinnableCampType = CampType;
        data.WinText = LanguageConfig.Instance.NeutralsWinText.CustomFormat(winner!.PlayerName);
        data.WinColor = Color;
        data.WinnablePlayers.Add(winner.Data);
        data.Winnable = true;
    }

    public uint GetWeight()
    {
        return IWinnable.GetOrder(5);
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnHostCheckStartMeeting(PlayerReportDeadBodyEvent @event)
    {
        if (!GameStates.InRealGame || GameStates.InLobby) return true;
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