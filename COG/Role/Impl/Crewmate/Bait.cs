using COG.Config.Impl;
using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Bait : Role, IListener
{
    public Bait() : base(LanguageConfig.Instance.BaitName, Color.blue, CampType.Crewmate)
    {
        Description = LanguageConfig.Instance.BaitDescription;
    }

    public void OnMurderPlayer(PlayerControl killer, PlayerControl target)
    {
        killer.CmdReportDeadBody(target.Data);
    }

    public override IListener GetListener(PlayerControl player)
    {
        return this;
    }
}