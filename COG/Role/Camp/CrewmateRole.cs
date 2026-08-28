using UnityEngine;

namespace COG.Role.Camp;

/// <summary>
/// Base class for crewmate roles. Automatically sets CampType to Crewmate.
/// </summary>
public abstract class CrewmateRole : CustomRole
{
    protected CrewmateRole(Color? color = null) 
        : base(color ?? Color.white, CampType.Crewmate) { }
}
