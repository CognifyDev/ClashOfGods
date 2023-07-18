using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl;

public class Impostor : Role
{
    public Impostor() : base("Impostor", Color.red, false, CampType.Impostor)
    {
        Description = "Kill the crewmates";
    }

    public override IListener GetListener(PlayerControl player)
    {
        return IListener.Empty;
    }
}