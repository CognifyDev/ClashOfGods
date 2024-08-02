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

namespace COG.Role;

/// <summary>
///     用来表示一个职业
/// </summary>
public abstract class CustomRole
{
    private static int _order;

    public CustomRole(string name, Color color, CampType campType, bool showInOptions = true)
    {
        Name = name;
        IsBaseRole = false;
        ShortDescription = "";
        Color = color;
        CampType = campType;
        BaseRoleType = campType == CampType.Impostor ? RoleTypes.Impostor : RoleTypes.Crewmate;
        IsSubRole = false;
        CanVent = campType == CampType.Impostor;
        CanKill = campType == CampType.Impostor;
        CanSabotage = campType == CampType.Impostor;
        Id = _order;
        _order++;
        ShowInOptions = showInOptions;
        AllOptions = new();
        var vanillaType = CampType switch
        {
            CampType.Crewmate => RoleTeamTypes.Crewmate,
            CampType.Impostor => RoleTeamTypes.Impostor,
            CampType.Neutral => (RoleTeamTypes)99,
            _ or CampType.Unknown => (RoleTeamTypes)100
        };
        VanillaRole = new RoleBehaviour
        {
            TeamType = vanillaType,
            Role = (RoleTypes)(Id + 100),
            StringName = StringNames.None
        };
        if (this is IWinnable winnable)
            CustomWinnerManager.RegisterWinnableInstance(winnable);

        if (ShowInOptions)
        {
            //                                  Actually name here is useless for new option
            RoleNumberOption = CreateOption(() => LanguageConfig.Instance.MaxNumMessage,
                new IntOptionValueRule(0, 1, 15, 1));
            RoleChanceOption = CreateOption(() => "Chance", new IntOptionValueRule(0, 10, 100, 100));
        }
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
    ///     角色名称
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     是否是基本职业
    /// </summary>
    public bool IsBaseRole { get; protected init; }

    /// <summary>
    ///     显示在职业分配后职业介绍界面的简短介绍文本
    /// </summary>
    public string ShortDescription { get; protected set; }

    /// <summary>
    ///     显示在职业设置的职业详细介绍文本
    /// </summary>
    public string LongDescription
    {
        get => string.IsNullOrEmpty(_longDescription) ? ShortDescription : _longDescription;
        set => _longDescription = value;
    }

    private string _longDescription = "";

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
    public bool IsSubRole { get; protected init; }

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

    public ReadOnlyCollection<PlayerControl> Players =>
        new(GameUtils.PlayerRoleData.Where(pr => pr.Player.IsRole(this)).Select(pr => pr.Player).ToList());

    public List<CustomOption> AllOptions { get; }

    /// <summary>
    ///     除了概率与人数之外的所有职业选项
    /// </summary>
    public ReadOnlyCollection<CustomOption> RoleOptions => new(AllOptions.Where(o => o != RoleNumberOption && o != RoleChanceOption).ToList());

    public RoleBehaviour VanillaRole { get; init; }

    public RoleRulesCategory VanillaCategory
    {
        get
        {
            var settings = new List<BaseGameSetting>();
            var idx = 0;
            foreach (var option in RoleOptions)
            {
                var rule = option.ValueRule;
                if (rule is BoolOptionValueRule)
                {
                    settings.Add(option.VanillaData = new CheckboxGameSetting()
                    {
                        Type = OptionTypes.Checkbox
                    });
                else if (rule is IntOptionValueRule iovr)
                {
                    settings.Add(option.VanillaData = new IntGameSetting()
                    {
                        Type = OptionTypes.Int,
                        Value = option.GetInt(),
                        Increment = iovr.Step,
                        ValidRange = new IntRange(iovr.Min, iovr.Max),
                        ZeroIsInfinity = false,
                        SuffixType = iovr.SuffixType,
                        FormatString = ""
                    });
                else if (rule is FloatOptionValueRule fovr)
                {
                    settings.Add(option.VanillaData = new FloatGameSetting()
                    {
                        Type = OptionTypes.Float,
                        Value = option.GetFloat(),
                        Increment = fovr.Step,
                        ValidRange = new FloatRange(fovr.Min, fovr.Max),
                        ZeroIsInfinity = false,
                        SuffixType = fovr.SuffixType,
                        FormatString = ""
                    });
                else if (rule is StringOptionValueRule sovr)
                {
                    settings.Add(option.VanillaData = new StringGameSetting()
                    {
                        Type = OptionTypes.String,
                        Index = option.Selection,
                        Values = new StringNames[sovr.Selections.Length]
                    });
                }

                idx++;
            }
            return new()
            {
                AllGameSettings = settings.ToIl2CppList(),
                Role = VanillaRole
            };
        }
    }

    protected CustomOption CreateOption(Func<string> nameGetter, IValueRule rule)
    {
        var option = CustomOption.Create(ToCustomOption(this), nameGetter, rule);
        AllOptions.Add(option);
        return option;
    }

    /// <summary>
    ///     添加一个按钮
    /// </summary>
    /// <param name="button">要添加的按钮</param>
    public void AddButton(CustomButton button)
    {
        button.HasButton += () =>
        {
            var player = PlayerControl.LocalPlayer;
            var role = player.GetMainRole();
            return role.Name.Equals(Name);
        };
        CustomButtonManager.GetManager().RegisterCustomButton(button);
    }

    public virtual string HandleEjectText(PlayerControl player)
    {
        var role = player.GetMainRole();
        var sb = new StringBuilder(role!.GetColorName());

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

    public virtual bool CanBeGiven(PlayerControl target)
    {
        return true;
    }

    public virtual void ClearRoleGameData()
    {
    }

    public string GetColorName()
    {
        return Name.Color(Color);
    }

    public static CustomOption.TabType ToCustomOption(CustomRole role)
    {
        if (role.CampType == CampType.Unknown || role.IsSubRole) return CustomOption.TabType.Addons;
        return (CustomOption.TabType)role.CampType;
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