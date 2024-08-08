using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Crewmate : CustomRole
{
    public Crewmate() : base(Color.white, CampType.Crewmate, false, false)
    {
        IsBaseRole = true;
    }

    public override IListener GetListener()
    {
        return IListener.EmptyListener;
    }

    public override CustomRole NewInstance()
    {
        return new Crewmate();
    }
}