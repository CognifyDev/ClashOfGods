using COG.Listener;
using UnityEngine;

namespace COG.Role.Impl;

public class Impostor : Role
{
    public Impostor() : base(2)
    {
        Name = "Impostor";
        Description = "Kill the crewmates";
        Color = Color.red;
    }

    public override IListener GetListener(PlayerControl player)
    {
        return IListener.Empty;
    }
}