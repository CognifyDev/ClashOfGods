using System.Collections;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using Reactor.Utilities;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Bait : COG.Role.Camp.CrewmateRole
{
    public Bait() : base(ColorUtils.AsColor("#00F7FF"))
    {
        KillerSelfReportDelay = CreateOption(() => GetContextFromLanguage("killer-report-delay"),
            new FloatOptionValueRule(0, 1, 5, 1, NumberSuffixes.Seconds));
        WarnKiller = CreateOption(() => GetContextFromLanguage("warn-killer"), new BoolOptionValueRule(true));

        On<PlayerMurderEvent>(OnMurderPlayer);
    }

    public CustomOption KillerSelfReportDelay { get; }
    public CustomOption WarnKiller { get; }

    private void OnMurderPlayer(PlayerMurderEvent @event)
    {
        var killer = @event.Player;
        var target = @event.Target;
        if (!killer.AmOwner) return; // Only the killer executes this
        if (!(killer && target)) return;
        if (target.IsRole(this)) Coroutines.Start(CoDelayedReport());

        IEnumerator CoDelayedReport()
        {
            var delay = KillerSelfReportDelay.GetFloat();
            var victim = target.Data; // Prevent exceptions when the bait quits

            yield return HudManager.Instance.CoFadeFullScreen(Color.clear, Color);
            yield return HudManager.Instance.CoFadeFullScreen(Color, Color.clear);

            if (delay != 0) yield return new WaitForSeconds(delay);
            killer.CmdReportDeadBody(victim);
        }
    }
}