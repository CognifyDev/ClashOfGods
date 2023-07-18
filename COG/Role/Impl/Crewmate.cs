using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl;

public class Crewmate : Role, IListener
{
    public Crewmate() : base("Crewmate", Color.white, false, CampType.Crewmate)
    {
        Description = "Finish your tasks!";
    }

    public override IListener GetListener(PlayerControl player)
    {
        return IListener.Empty;
    }
}