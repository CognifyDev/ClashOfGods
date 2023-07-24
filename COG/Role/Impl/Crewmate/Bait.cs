using COG.Config.Impl;
using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Bait : Role, IListener
{
    private PlayerControl? _player;
    
    public Bait() : base(LanguageConfig.Instance.BaitName, Color.blue, CampType.Crewmate)
    {
        SubRole = true;
    }

    public void OnMurderPlayer(PlayerControl killer, PlayerControl target)
    {
        killer.CmdReportDeadBody(target.Data);
    }

    public override IListener GetListener(PlayerControl player)
    {
        _player = player;
        return this;
    }
}