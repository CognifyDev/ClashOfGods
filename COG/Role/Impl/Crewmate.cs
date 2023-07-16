using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl;

public class Crewmate : Role, IListener
{
    public Crewmate() : base(1)
    {
        Name = "Crewmate";
        Description = "Finish your tasks!";
        Color = Color.white;
        CampType = CampType.Crewmate;
    }

    public override IListener GetListener(PlayerControl player)
    {
        return IListener.Empty;
    }
}