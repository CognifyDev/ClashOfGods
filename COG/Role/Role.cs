using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using UnityEngine;

namespace COG.Role;

/// <summary>
///     用来表示一个职业
/// </summary>
public class Role
{
    public Role(string name, Color color, CampType campType, bool showInOptions)
    {
        Name = name;
        BaseRole = false;
        Description = "";
        Color = color;
        CampType = campType;
        BaseRoleType = RoleTypes.Crewmate;
        SubRole = false;
        CanVent = campType == CampType.Impostor;
        CanKill = campType == CampType.Impostor;
        CanSabotage = campType == CampType.Impostor;
        Id = RoleManager.GetManager().GetAvailableRoleId();
        ShowInOptions = showInOptions;

        if (ShowInOptions)
        {
            MainRoleOption = CustomOption.Create(false, ToCustomOption(this),
                ColorUtils.ToColorString(Color, Name), false, null, true);
            RoleNumberOption = CustomOption.Create(false, ToCustomOption(this),
                LanguageConfig.Instance.MaxNumMessage, 1, 1, 15, 1, MainRoleOption);
        }
    }

    /// <summary>
    ///     角色特征码
    /// </summary>
    public uint Id { get; }

    /// <summary>
    ///     角色颜色
    /// </summary>
    public Color Color { get; }

    /// <summary>
    ///     角色名称
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    ///     是否是基本职业
    /// </summary>
    public bool BaseRole { get; protected init; }

    /// <summary>
    ///     角色介绍
    /// </summary>
    public string Description { get; protected init; }

    /// <summary>
    ///     角色阵营
    /// </summary>
    public CampType CampType { get; }

    /// <summary>
    ///     原版角色蓝本
    /// </summary>
    public RoleTypes BaseRoleType { get; protected init; }

    /// <summary>
    ///     是否为副职业
    /// </summary>
    public bool SubRole { get; }

    /// <summary>
    ///     在选项中显示
    /// </summary>
    public bool ShowInOptions { get; }

    /// <summary>
    ///     是否可以跳管
    /// </summary>
    public bool CanVent { get; protected init; }

    /// <summary>
    ///     是否可以击杀
    /// </summary>
    public bool CanKill { get; protected init; }

    /// <summary>
    ///     是否可以破坏
    /// </summary>
    public bool CanSabotage { get; protected init; }

    /// <summary>
    ///     角色的Option
    /// </summary>
    public CustomOption? MainRoleOption { get; }

    /// <summary>
    ///     选择角色数量的option
    /// </summary>
    public CustomOption? RoleNumberOption { get; }

    public List<PlayerControl> Players => GameUtils.PlayerRoleData.Where(pr => pr.Role == this).Select(pr => pr.Player).ToList();

    /// <summary>
    ///     添加一个按钮
    /// </summary>
    /// <param name="button">要添加的按钮</param>
    public void AddButton(CustomButton button)
    {
        button.HasButton ??= () => true;

        button.HasButton += () =>
        {
            var player = PlayerControl.LocalPlayer;
            var role = player.GetRoleInstance();
            return role != null && role.Name.Equals(Name);
        };
        CustomButtonManager.GetManager().RegisterCustomButton(button);
    }

    public static CustomOption.CustomOptionType ToCustomOption(Role role)
    {
        if (role.CampType == CampType.Unknown || role.SubRole) return CustomOption.CustomOptionType.Addons;
        return (CustomOption.CustomOptionType)role.CampType;
    }

    public virtual IListener GetListener() => IListener.EmptyListener;
}
