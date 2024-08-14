using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Listener;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;
using Random = System.Random;

// ReSharper disable Unity.IncorrectScriptableObjectInstantiation

namespace COG.Role;

/// <summary>
///     用来表示一个职业
/// </summary>
public abstract class CustomRole
{
    private static int _order;
    
    public CustomRole(Color color, CampType campType, bool isSubRole = false, bool showInOptions = true)
    {
        IsBaseRole = false;
        Color = color;
        CampType = campType;
        BaseRoleType = campType == CampType.Impostor ? RoleTypes.Impostor : RoleTypes.Crewmate;
        IsSubRole = isSubRole;
        CanVent = campType == CampType.Impostor;
        CanKill = campType == CampType.Impostor;
        CanSabotage = campType == CampType.Impostor;
        Id = _order;
        _order++;
        ShowInOptions = showInOptions;
        AllOptions = new List<CustomOption>();
        
        Name = GetContextFromLanguage("name");
        ShortDescription = GetContextFromLanguage("description");
        
        var vanillaType = CampType switch
        {
            CampType.Crewmate => RoleTeamTypes.Crewmate,
            CampType.Impostor => RoleTeamTypes.Impostor,
            CampType.Neutral => (RoleTeamTypes)99,
            _ or CampType.Unknown => (RoleTeamTypes)100
        };
        // ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
        VanillaRole = new RoleBehaviour
        {
            TeamType = vanillaType,
            Role = (RoleTypes)(Id + 100),
            StringName = StringNames.None
        };

        if (this is IWinnable winnable)
        {
            CustomWinnerManager.GetManager().RegisterCustomWinnable(winnable);
        }

        if (ShowInOptions)
        {
            // Actually name here is useless for new option
            RoleNumberOption = CreateOption(() => LanguageConfig.Instance.MaxNumMessage,
                new IntOptionValueRule(0, 1, 15, 0));
            RoleChanceOption = CreateOption(() => "Chance", new IntOptionValueRule(0, 10, 100, 0));
            
            RoleCode = CreateOption(() => LanguageConfig.Instance.RoleCode, 
                new StringOptionValueRule(0, _ => new[] {Id.ToString()}));
        }
    }

#pragma warning disable CS0659
    public override bool Equals(object? obj)
#pragma warning restore CS0659
    {
        if (obj is not CustomRole role) return false;
        return role.Id == Id;
    }

    /// <summary>
    ///     角色特征码
    /// </summary>
    public int Id { get; }

    /// <summary>
    ///     角色颜色
    /// </summary>
    public Color Color { get; }
    
    /// <summary>
    /// 角色的名称
    /// </summary>
    public string Name { get; }
    
    private string GetContextFromLanguage(string context)
    {
        var campName = IsSubRole ? "sub-roles" : Enum.GetName(typeof(CampType), CampType)!.ToLower();
        var location = $"role.{campName}.{GetNameInConfig()}.{context}";
        var toReturn = LanguageConfig.Instance.YamlReader!
            .GetString(location);
        return toReturn ?? LanguageConfig.Instance.NoMoreDescription;
    }

    /// <summary>
    ///     是否是基本职业
    /// </summary>
    public bool IsBaseRole { get; protected init; }

    /// <summary>
    ///     显示在职业分配后职业介绍界面的简短介绍文本
    /// </summary>
    public string ShortDescription { get; protected set; }

    /// <summary>
    /// 显示在职业设置的职业详细介绍文本
    /// </summary>
    /// <returns>详细介绍</returns>
    public string GetLongDescription()
    {
        return GetContextFromLanguage("long-description");
    }

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
    public bool IsSubRole { get; }

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
    ///     选择角色数量的option
    /// </summary>
    public CustomOption? RoleNumberOption { get; }

    /// <summary>
    ///     职业几率的选项
    /// </summary>
    public CustomOption? RoleChanceOption { get; }

    public CustomOption? RoleCode { get; }

    /// <summary>
    ///     职业是否已启用
    /// </summary>
    public bool Enabled
    {
        get
        {
            if (RoleNumberOption != null) return RoleNumberOption!.GetInt() > 0;
            return false;
        }
    }

    public bool IsAvailable()
    {
        if (!Enabled || IsBaseRole || !ShowInOptions) return false;
        var chance = RoleChanceOption?.GetInt();
        
        if (chance == null)
        {
            return false;
        }

        return new Random().Next(0, 100) <= chance;
    }

    public string GetNormalName() => GetType().Name;

    public virtual string GetNameInConfig() => GetType().Name.ToLower();

    public ReadOnlyCollection<PlayerControl> Players =>
        new(GameUtils.PlayerData.Where(pr => pr.Player.IsRole(this)).Select(pr => pr.Player).ToList());

    public List<CustomOption> AllOptions { get; }

    /// <summary>
    ///     除了概率与人数之外的所有职业选项
    /// </summary>
    public ReadOnlyCollection<CustomOption> RoleOptions =>
        new(AllOptions.Where(o => o != RoleNumberOption && o != RoleChanceOption).ToList());

    public RoleBehaviour VanillaRole { get; }

    public RoleRulesCategory VanillaCategory => new()
    {
        AllGameSettings = RoleOptions.Select(o => o.ToVanillaOptionData()).ToList().ToIl2CppList(),
        Role = VanillaRole
    };

    protected CustomOption CreateOption(Func<string> nameGetter, IValueRule rule)
    {
        var option = CustomOption.Of(GetTabType(this), nameGetter, rule).Register();
        AllOptions.Add(option);
        return option;
    }

    protected void RegisterCustomOption(CustomOption option)
    {
        AllOptions.Add(option.Register());
    }

    public bool IsPlayerControlRole(PlayerControl target)
    {
        return PlayerControl.LocalPlayer.IsSamePlayer(target) && target.IsRole(this);
    }

    protected CustomOption CreateOptionWithoutRegister(Func<string> nameGetter, IValueRule rule)
    {
        return CustomOption.Of(GetTabType(this), nameGetter, rule);
    }

    /// <summary>
    ///     添加一个按钮
    /// </summary>
    /// <param name="button">要添加的按钮</param>
    public void AddButton(CustomButton button, Func<bool>? hasButton = null)
    {
        hasButton ??= () => PlayerControl.LocalPlayer.IsRole(this);
        button.HasButton += hasButton;
        CustomButtonManager.GetManager().RegisterCustomButton(button);
    }

    public virtual string HandleEjectText(PlayerControl player)
    {
        var role = player.GetMainRole();
        var sb = new StringBuilder(role.GetColorName());

        foreach (var subRole in player.GetSubRoles())
            sb.Append(' ').Append(subRole.GetColorName());

        return LanguageConfig.Instance.DefaultEjectText.CustomFormat(player.Data.PlayerName, sb.ToString());
    }

    public virtual string HandleAdditionalPlayerName()
    {
        return "";
    }

    /// <summary>
    ///     改写在分配该职业时的逻辑
    /// </summary>
    /// <param name="roles">职业列表</param>
    /// <returns>如果返回true，则跳过根据 <seealso cref="RoleNumberOption" /> 添加此职业待分配数量而仅执行该方法的逻辑</returns>
    public virtual bool OnRoleSelection(List<CustomRole> roles)
    {
        return false;
    }

    public virtual void AfterSharingRoles()
    {
    }

    public virtual void ClearRoleGameData()
    {
    }

    public string GetColorName()
    {
        return Name.Color(Color);
    }

    public static CustomOption.TabType GetTabType(CustomRole role)
    {
        if (role.CampType == CampType.Unknown || role.IsSubRole) return CustomOption.TabType.Addons;
        return role.CampType switch
        {
            CampType.Crewmate => CustomOption.TabType.Crewmate,
            CampType.Impostor => CustomOption.TabType.Impostor,
            CampType.Neutral => CustomOption.TabType.Neutral,
            _ => CustomOption.TabType.Addons,
        };
    }

    public virtual IListener GetListener()
    {
        return IListener.EmptyListener;
    }

    public abstract CustomRole NewInstance();

    ~CustomRole()
    {
        ClearRoleGameData();
    }
}