using UnityEngine;

namespace COG.Role.Camp;

/// <summary>
/// Base class for impostor roles. Automatically sets CampType to Impostor and enables Kill/Vent/Sabotage.
/// </summary>
public abstract class ImpostorRole : CustomRole
{
    protected ImpostorRole(Color? color = null) 
        : base(color ?? Palette.ImpostorRed, CampType.Impostor)
    {
        SetImpostorCapabilities();
    }

    private void SetImpostorCapabilities()
    {
        CanKill = true;
        CanVent = true;
        CanSabotage = true;
    }
}
