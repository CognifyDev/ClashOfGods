global using Crewmate = COG.Role.Impl.Crewmate.Crewmate;
using COG.Listener;

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