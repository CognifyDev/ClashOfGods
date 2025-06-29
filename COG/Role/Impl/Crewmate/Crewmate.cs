using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Crewmate : CustomRole
{
    public Crewmate() : base(Palette.CrewmateBlue, CampType.Crewmate, false)
    {
        IsBaseRole = true;
    }

    public override IListener GetListener()
    {
        return IListener.EmptyListener;
    }
}