using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Game.CustomWinner;
using COG.Game.Events;
using COG.Listener;
using COG.Listener.Event.Impl.Game.Record;
using COG.Rpc;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.UI.Vanilla.KillButton;
using COG.Utils;
using UnityEngine;
using Random = System.Random;

// ReSharper disable Unity.IncorrectScriptableObjectInstantiation

namespace COG.Role;

#pragma warning disable CS0659
/*
 WARNING:
  Most of the members in CustomRole and PlayerControl won't synchronize automatically,
  so you probably gotta use RPC to synchronize them.
 */
/// <summary>
///     用来表示一个职业
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class CustomRole
{
    private static int _order;
    private KillButtonSetting _currentKillButtonSetting;
    private readonly Stack<KillButtonSetting> _killButtonSettings = new();

    /// <summary>
    ///     Initializes a sub-role instance.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="showInOptions"></param>
    public CustomRole(Color color, bool showInOptions = true) : this(color, CampType.Unknown, true, showInOptions)
    {
    }

    /// <summary>
    ///     Initializes a main role instance.
    /// </summary>
    /// <param name="color"></param>
    /// <param name="campType"></param>
    /// <param name="showInOptions"></param>
    public CustomRole(Color color, CampType campType, bool showInOptions = true) : this(color, campType, false,
        showInOptions)
    {
    }

    /// <summary>
    ///     Initializes an impostor role instance.
    /// </summary>
    /// <param name="showInOptions"></param>
    public CustomRole(bool showInOptions = true) : this(Palette.ImpostorRed, CampType.Impostor, showInOptions)
    {
    }

    private CustomRole(Color color, CampType campType, bool isSubRole, bool showInOptions)
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
        AllOptions = [];

        _currentKillButtonSetting = DefaultKillButtonSetting = new KillButtonSetting
        {
            ForceShow = () => CanKill,
            TargetOutlineColor = Color
        };
        DefaultKillButtonSetting.AddAfterClick(() => OnRoleAbilityUsed(this, null!));

        ResetCurrentKillButtonSetting();

        Name = GetContextFromLanguage("name");
        ShortDescription = GetContextFromLanguage("description");
        ActionNameContext = LanguageConfig.Instance.GetHandler("action");

        if (this is IWinnable winnable) CustomWinnerManager.GetManager().RegisterCustomWinnable(winnable);

        if (ShowInOptions)
        {
            // Actually name here is useless for new option
            RoleNumberOption = CreateOption(() => LanguageConfig.Instance.MaxNumMessage,
                new IntOptionValueRule(0, 1, 15, 0));
            RoleChanceOption = CreateOption(() => "Chance",
                new IntOptionValueRule(0, 10, 100, 0));

            RoleCode = CreateOption(() => LanguageConfig.Instance.RoleCode,
                new StringOptionValueRule(0, _ => Id.ToString().ToSingleElementArray()));
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
    ///     角色的名称
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
    public CustomOption? RoleChanceOption { get; internal set; }

    public CustomOption? RoleCode { get; internal set; }

    /// <summary>
    ///     职业是否已启用
    /// </summary>
    public bool Enabled
    {
        get
        {
            if (RoleNumberOption != null)
                return RoleNumberOption!.GetInt() > 0;
            return false;
        }
    }

    public LanguageConfig.TextHandler ActionNameContext { get; }

    /// <summary>
    ///     WARNING: Only local player performs this.
    /// </summary>
    public static Action<CustomRole, CustomButton> OnRoleAbilityUsed { get; set; } = (_, button) =>
    {
        EventRecorder.Instance.RpcRecord<UseAbilityGameEvent, UseAbilityEventSender>(new UseAbilityGameEvent(PlayerControl.LocalPlayer.GetPlayerData(), button));
    };

    public ReadOnlyCollection<PlayerControl> Players =>
        new(GameUtils.PlayerData.Where(pr => !pr.IsDisconnected && pr.Player.IsRole(this)).Select(pr => pr.Player)
            .ToList());

    public List<CustomOption> AllOptions { get; internal set; }

    /// <summary>
    ///     除了概率与人数之外的所有职业选项
    /// </summary>
    public ReadOnlyCollection<CustomOption> RoleOptions =>
        new(AllOptions.Where(o => o != RoleNumberOption && o != RoleChanceOption).ToList());

    public RoleBehaviour VanillaRole => new()
    {
        TeamType = CampType switch
        {
            CampType.Crewmate => RoleTeamTypes.Crewmate,
            CampType.Impostor => RoleTeamTypes.Impostor,
            CampType.Neutral => (RoleTeamTypes)99,
            _ => (RoleTeamTypes)100
        },
        Role = (RoleTypes)(Id + 100),
        StringName = StringNames.None,
        AllGameSettings = RoleOptions.Select(o => o.ToVanillaOptionData()).ToIl2CppList()
    };

    public KillButtonSetting DefaultKillButtonSetting { get; }

    /// <summary>
    ///     NOTE: Set to null to use previous setting.
    ///     <para />
    ///     PLEASE CAREFULLY CONSIDER WHETHER TO CLONE ONE OR TO JUST OVERRIDE!
    /// </summary>
    public KillButtonSetting CurrentKillButtonSetting
    {
        get => _currentKillButtonSetting;
        set
        {
            if (value == null!)
            {
                if (_killButtonSettings.Count > 0)
                {
                    _currentKillButtonSetting = _killButtonSettings.Pop();
                }
                else
                {
                    _currentKillButtonSetting = DefaultKillButtonSetting;
                    _killButtonSettings.Push(DefaultKillButtonSetting);
                }
            }
            else
            {
                _killButtonSettings.Push(_currentKillButtonSetting);
                _currentKillButtonSetting = value;
            }
        }
    }

    public List<CustomButton> AllButtons { get; } = new();

    public override bool Equals(object? obj)
    {
        if (obj is not CustomRole role) return false;
        return role.Id == Id;
    }

    protected string GetContextFromLanguage(string context)
    {
        var campName = IsSubRole ? "sub-roles" : CampType.ToString().ToLower();
        var location = $"role.{campName}.{GetNameInConfig()}.{context}";
        var toReturn = LanguageConfig.Instance.YamlReader!.GetString(location);
        return toReturn ?? LanguageConfig.Instance.NoMoreDescription;
    }

    /// <summary>
    ///     显示在职业设置的职业详细介绍文本
    /// </summary>
    /// <returns>详细介绍</returns>
    public string GetLongDescription()
    {
        return GetContextFromLanguage("long-description");
    }

    public bool IsAvailable()
    {
        if (!Enabled || IsBaseRole || !ShowInOptions) return false;
        var chance = RoleChanceOption?.GetInt();

        if (chance == null) return false;

        return new Random().Next(0, 100) <= chance;
    }

    public string GetNormalName()
    {
        return GetType().Name;
    }

    public virtual string GetNameInConfig()
    {
        return GetType().Name.ToLower();
    }

    protected CustomOption CreateOption(Func<string> nameGetter, IValueRule rule)
    {
        if (!ShowInOptions) return null!;

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

    protected CustomOption CreateOptionWithoutRegisteration(Func<string> nameGetter, IValueRule rule)
    {
        return CustomOption.Of(GetTabType(this), nameGetter, rule);
    }

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
            button.OnEffect += () => OnRoleAbilityUsed(this, button);
        else
            button.OnClick += () => OnRoleAbilityUsed(this, button);

        CustomButtonManager.GetManager().RegisterCustomButton(button);

        button.Text =
            button.Text.Color(Color); // However because of the material of the font, the color string doesnt work
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

    public virtual void OnRoleGameDataSynchronizing(RpcWriter writer)
    {
    }

    public virtual void OnRpcReceived(PlayerControl sender, byte callId, MessageReader reader)
    {
    }

    public virtual void OnUpdate()
    {
    }


    public void SyncRoleGameData()
    {
        var writer = RpcWriter.Start(KnownRpc.SyncRoleGameData).WritePacked(Id);
        OnRoleGameDataSynchronizing(writer);
        writer.Finish();
    }

    public string GetColorName()
    {
        return Name.Color(Color);
    }

    public void RegisterRpcHandler(IRpcHandler handler)
    {
        IRpcHandler.Register(handler);
    }

    public void ResetCurrentKillButtonSetting()
    {
        CurrentKillButtonSetting = null!;
    }


    public static void ClearKillButtonSettings()
    {
        CustomRoleManager.GetManager().GetRoles().ForEach(r =>
        {
            r._killButtonSettings.Clear();
            r.ResetCurrentKillButtonSetting();
        });
    }

    public static CustomOption.TabType GetTabType(CustomRole role)
    {
        if (role.CampType == CampType.Unknown || role.IsSubRole) return CustomOption.TabType.Addons;
        return role.CampType switch
        {
            CampType.Crewmate => CustomOption.TabType.Crewmate,
            CampType.Impostor => CustomOption.TabType.Impostor,
            CampType.Neutral => CustomOption.TabType.Neutral,
            _ => CustomOption.TabType.Addons
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

#pragma warning restore CS0659