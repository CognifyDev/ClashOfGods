using AmongUs.GameOptions;
using UnityEngine;

namespace COG.Role.Components;

/// <summary>
///     Role identity and display information.
/// </summary>
public class RoleMetadata
{
    /// <summary>
    ///     Role identifier (characteristic code).
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     Role color.
    /// </summary>
    public Color Color { get; }

    /// <summary>
    ///     Role name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Short description displayed in the role introduction screen after role assignment.
    /// </summary>
    public string ShortDescription { get; set; }

    /// <summary>
    ///     Role camp type.
    /// </summary>
    public CampType CampType { get; }

    /// <summary>
    ///     Vanilla role type template.
    /// </summary>
    public RoleTypes BaseRoleType { get; internal set; }

    /// <summary>
    ///     Whether this is a sub-role.
    /// </summary>
    public bool IsSubRole { get; }

    /// <summary>
    ///     Whether this is a base role.
    /// </summary>
    public bool IsBaseRole { get; set; }

    /// <summary>
    ///     Whether to show this role in options.
    /// </summary>
    public bool ShowInOptions { get; }

    public RoleMetadata(int id, string name, Color color, CampType campType,
        bool isSubRole = false, bool showInOptions = true)
    {
        Id = id;
        Name = name;
        ShortDescription = "";
        Color = color;
        CampType = campType;
        BaseRoleType = campType == CampType.Impostor ? RoleTypes.Impostor : RoleTypes.Crewmate;
        IsSubRole = isSubRole;
        ShowInOptions = showInOptions;
    }

    /// <summary>
    ///     Returns the role name colored with the role's color.
    /// </summary>
    public string GetColorName() => Name.Color(Color);
}
