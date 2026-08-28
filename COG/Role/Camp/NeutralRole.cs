using UnityEngine;

namespace COG.Role.Camp;

/// <summary>
/// Base class for neutral roles. Requires explicit color.
/// </summary>
public abstract class NeutralRole : CustomRole
{
    protected NeutralRole(Color color) : base(color, CampType.Neutral) { }
}
