using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using Reactor.Utilities;
using System.Collections;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Bait : CustomRole, IListener
{
    public CustomOption KillerSelfReportDelay { get; }
    public CustomOption WarnKiller { get; }

    public Bait() : base(ColorUtils.AsColor("#00F7FF"), CampType.Crewmate)
    {
        KillerSelfReportDelay = CreateOption(() => GetContextFromLanguage("killer-report-delay"), 
            new FloatOptionValueRule(0, 1, 5, 1));
        WarnKiller = CreateOption(() => GetContextFromLanguage("warn-killer"), new BoolOptionValueRule(true));
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnMurderPlayer(PlayerMurderEvent @event)
    {
        var killer = @event.Player;
        var target = @event.Target;
        if (!IsLocalPlayerRole(target)) return;
        if (killer == null || target == null) return;
        if (target.IsRole(this)) Coroutines.Start(CoDelayedReport());

        IEnumerator CoDelayedReport()
        {
            var delay = KillerSelfReportDelay.GetFloat();
            var victim = target.Data; // Prevent exceptions when the bait quits
            if (delay != 0) yield return new WaitForSeconds(delay);
            killer.CmdReportDeadBody(victim);
        }
    }

    public override IListener GetListener()
    {
        return this;
    }

    public override CustomRole NewInstance()
    {
        return new Bait();
    }
}