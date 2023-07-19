using System.Collections.Generic;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.Modules;
using COG.Utils;
using UnityEngine;

namespace COG.Role;

/// <summary>
/// 用来表示一个职业
/// </summary>
public abstract class Role
{
    /// <summary>
    /// 角色颜色
    /// </summary>
    public Color Color { get; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// 角色介绍
    /// </summary>
    public string Description { get; protected set; }

    /// <summary>
    /// 角色阵营
    /// </summary>
    public CampType CampType { get; }
    
    /// <summary>
    /// 原版角色蓝本
    /// </summary>
    public RoleTypes BaseRoleType { get; protected set; }
    
    /// <summary>
    /// 角色设置
    /// </summary>
    public List<CustomOption> RoleOptions { get; }
    
    /// <summary>
    /// 是否在设置中显示该职业
    /// </summary>
    public bool DisplayInOption { get; }

    protected Role(string name, Color color, bool displayInOption, CampType campType)
    {
        DisplayInOption = displayInOption;
        Name = name;
        Description = "";
        Color = color;
        CampType = campType;
        BaseRoleType = RoleTypes.Crewmate;
        RoleOptions = new();

        if (DisplayInOption)
        {
            var option = CustomOption.Create(Name.GetHashCode(), ToCustomOption(this),
                ColorUtils.ToAmongUsColorString(Color, Name), false, null, true);
            RoleOptions.Add(option);
            RoleOptions.Add(CustomOption.Create(Name.GetHashCode() * Name.GetHashCode(), ToCustomOption(this),
                LanguageConfig.Instance.MaxNumMessage, 1, 1, 15, 1, option));
        }
    }

    public static CustomOption.CustomOptionType ToCustomOption(Role role)
    {
        return (CustomOption.CustomOptionType) role.CampType;
    }

    public abstract IListener GetListener(PlayerControl player);
}