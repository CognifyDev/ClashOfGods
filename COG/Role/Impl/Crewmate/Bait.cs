using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.Utils;

namespace COG.Role.Impl.Crewmate;

public class Bait : CustomRole, IListener
{
    public Bait() : base(LanguageConfig.Instance.BaitName, ColorUtils.AsColor("#00F7FF"), CampType.Crewmate)
    {
        ShortDescription = LanguageConfig.Instance.BaitDescription;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnMurderPlayer(PlayerMurderEvent @event)
    {
        var killer = @event.Player;
        var target = @event.Target;
        if (killer == null || target == null) return;
        if (target.IsRole(this)) killer.CmdReportDeadBody(target.Data);
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