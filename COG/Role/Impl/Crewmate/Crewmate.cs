using COG.Config.Impl;
using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Crewmate : Role
{
    public Crewmate() : base(LanguageConfig.Instance.CrewmateName, Color.white, CampType.Crewmate, true)
    {
        Description = LanguageConfig.Instance.CrewmateDescription;
    }

    public override IListener GetListener(PlayerControl player)
    {
        return IListener.Empty;
    }
}