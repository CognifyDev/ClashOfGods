using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Listener;
using COG.Rpc;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Vanilla.KillButton;
using COG.Utils;
using UnityEngine;
using Random = System.Random;

// ReSharper disable Unity.IncorrectScriptableObjectInstantiation

namespace COG.Role;

#pragma warning disable CS0659

/// <summary>
///     用来表示一个职业
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class CustomRole
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
        Id = _order++;
        ShowInOptions = showInOptions;
        AllOptions = new List<CustomOption>();
        KillButtonSetting = new()
        {
            ForceShow = () => CanKill
        };
        
        Name = GetContextFromLanguage("name");
        ShortDescription = GetContextFromLanguage("description");
        
        var vanillaType = CampType switch
        {
            CampType.Crewmate => RoleTeamTypes.Crewmate,
            CampType.Impostor => RoleTeamTypes.Impostor,
            CampType.Neutral => (RoleTeamTypes)99,
            _ => (RoleTeamTypes)100
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

    public override bool Equals(object? obj)
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
    
    protected string GetContextFromLanguage(string context)
    {
        var campName = IsSubRole ? "sub-roles" : CampType.ToString().ToLower();
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
    public CustomOption? RoleNumberOption { get; internal set; }

    /// <summary>
    ///     职业几率的选项
    /// </summary>
    public CustomOption? RoleChanceOption { get; internal set;  }

    public CustomOption? RoleCode { get; internal set; }

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

    public static Action<CustomRole> OnRoleAbilityUsed { get; set; } = (_) => { };

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
        new(GameUtils.PlayerData.Where(pr => !pr.IsDisconnected && pr.Player.IsRole(this)).Select(pr => pr.Player).ToList());

    public List<CustomOption> AllOptions { get; internal set; }

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

    public KillButtonSetting KillButtonSetting { get; }

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

    public bool IsLocalPlayerRole(PlayerControl target)
    {
        return PlayerControl.LocalPlayer.IsSamePlayer(target) && target.IsRole(this);
    }

    public bool IsLocalPlayerRole()
    {
        return PlayerControl.LocalPlayer.IsRole(this);
    }

    protected CustomOption CreateOptionWithoutRegister(Func<string> nameGetter, IValueRule rule)
    {
        return CustomOption.Of(GetTabType(this), nameGetter, rule);
    }

    public List<CustomButton> AllButtons { get; } = new();

    /// <summary>
    ///     添加一个按钮
    /// </summary>
    /// <param name="button">要添加的按钮</param>
    /// <param name="hasButton"></param>
    public void AddButton(CustomButton button, Func<bool>? hasButton = null)
    {
        hasButton ??= () => PlayerControl.LocalPlayer.IsRole(this);
        button.HasButton += hasButton;
        if (button.HasEffect)
            button.OnEffect += () => OnRoleAbilityUsed(this);
        else
            button.OnClick += () => OnRoleAbilityUsed(this);

        CustomButtonManager.GetManager().RegisterCustomButton(button);

        button.Text = button.Text.Color(Color); // However because of the material of the font, the color string doesnt work
        AllButtons.Add(button);
    }

    public virtual string HandleEjectText(NetworkedPlayerInfo player)
    {
        var role = player.GetMainRole();
        var sb = new StringBuilder(role.GetColorName());

        foreach (var subRole in player.GetSubRoles())
            sb.Append(' ').Append(subRole.GetColorName());

        return LanguageConfig.Instance.DefaultEjectText.CustomFormat(player.PlayerName, sb.ToString());
    }

    public virtual string HandleAdditionalPlayerName(PlayerControl player)
    {
        return "";
    }

    public virtual void AfterSharingRoles()
    {
    }

    public virtual void ClearRoleGameData()
    {
    }

    public virtual void OnRoleGameDataGettingSynchronized(MessageReader reader)
    {
    }

    public virtual void OnRoleGameDataBeingSynchronized(RpcWriter writer)
    {
    }

    public void SyncRoleGameData()
    {
        var writer = RpcUtils.StartRpcImmediately(KnownRpc.SyncRoleGameData).WritePacked(Id);
        OnRoleGameDataBeingSynchronized(writer);
        writer.Finish();
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
    
    ~CustomRole()
    {
        ClearRoleGameData();
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class OnlyLocalPlayerWithThisRoleInvokableAttribute : Attribute
{
}

#pragma warning restore CS0659