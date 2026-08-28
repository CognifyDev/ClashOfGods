namespace COG.Role.Components;

/// <summary>
///     Defines what actions a role can perform.
/// </summary>
public class RoleCapabilities
{
    /// <summary>
    ///     Whether the role can use vents.
    /// </summary>
    public bool CanVent { get; set; }

    /// <summary>
    ///     Whether the role can kill.
    /// </summary>
    public bool CanKill { get; set; }

    /// <summary>
    ///     Whether the role can use sabotage.
    /// </summary>
    public bool CanSabotage { get; set; }

    public RoleCapabilities(bool canVent = false, bool canKill = false, bool canSabotage = false)
    {
        CanVent = canVent;
        CanKill = canKill;
        CanSabotage = canSabotage;
    }
}
