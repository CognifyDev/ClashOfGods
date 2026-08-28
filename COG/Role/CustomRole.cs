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
using COG.Rpc.Role;
using COG.Role.Components;
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
///     Represents a custom role with identity, capabilities, buttons, and options.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class CustomRole
{
    private static int _order;

    // Component instances
    private readonly RoleMetadata _metadata;
    private readonly RoleCapabilities _capabilities;
    private readonly RoleButtons _buttons;
    private readonly RoleOptions _options;

    // Role-scoped RPC dispatch table: localRpcId → typed handler
    // Populated by NewRpc(); used by DispatchRoleRpc.
    private readonly Dictionary<uint, IRoleRpc> _roleRpcHandlers = new();

    /// <summary>
    ///     Initializes a sub-role instance.
    /// </summary>
    public CustomRole(Color color, bool showInOptions = true) : this(color, CampType.Unknown, true, showInOptions)
    {
    }

    /// <summary>
    ///     Initializes a main role instance.
    /// </summary>
    public CustomRole(Color color, CampType campType, bool showInOptions = true) : this(color, campType, false,
        showInOptions)
    {
    }

    /// <summary>
    ///     Initializes an impostor role instance.
    /// </summary>
    public CustomRole(bool showInOptions = true) : this(Palette.ImpostorRed, CampType.Impostor, showInOptions)
    {
    }

    private CustomRole(Color color, CampType campType, bool isSubRole, bool showInOptions)
    {
        var id = _order++;

        _metadata = new RoleMetadata(id, "", color, campType, isSubRole, showInOptions)
        {
            IsBaseRole = false
        };

        _capabilities = new RoleCapabilities(
            canVent: campType == CampType.Impostor,
            canKill: campType == CampType.Impostor,
            canSabotage: campType == CampType.Impostor
        );

        _buttons = new RoleButtons();
        _buttons.DefaultKillButtonSetting.ForceShow = () => CanKill;
        _buttons.DefaultKillButtonSetting.TargetOutlineColor = Color;
        _buttons.DefaultKillButtonSetting.AddAfterClick(() => OnRoleAbilityUsed(this, null!));
        _buttons.ResetCurrentKillButtonSetting();

        _options = new RoleOptions();

        Name = GetContextFromLanguage("name");
        ShortDescription = GetContextFromLanguage("description");
        ActionNameContext = LanguageConfig.Instance.GetHandler("action");

        if (this is IWinnable winnable) CustomWinnerManager.GetManager().RegisterCustomWinnable(winnable);

        if (ShowInOptions)
        {
            // Actually name here is useless for new option
            RoleNumberOption = CreateOption(() => LanguageConfig.Instance.GetString("role.global.max-num"),
                new IntOptionValueRule(0, 1, 15, 0));
            RoleChanceOption = CreateOption(() => "Chance",
                new IntOptionValueRule(0, 10, 100, 0));

            RoleCode = CreateOption(() => LanguageConfig.Instance.GetString("role.global.role-code"),
                new StringOptionValueRule(0, _ => Id.ToString().ToSingleElementArray()));
        }
    }

    // ==================== Metadata Delegates ====================

    /// <summary>
    ///     Role identifier (characteristic code).
    /// </summary>
    public int Id => _metadata.Id;

    /// <summary>
    ///     Role color.
    /// </summary>
    public Color Color => _metadata.Color;

    /// <summary>
    ///     Role name.
    /// </summary>
    public string Name
    {
        get => _metadata.Name;
        protected set => _metadata.Name = value;
    }

    /// <summary>
    ///     Whether this is a base role.
    /// </summary>
    public bool IsBaseRole
    {
        get => _metadata.IsBaseRole;
        protected init => _metadata.IsBaseRole = value;
    }

    /// <summary>
    ///     Short description displayed in the role introduction screen after role assignment.
    /// </summary>
    public string ShortDescription
    {
        get => _metadata.ShortDescription;
        protected set => _metadata.ShortDescription = value;
    }

    /// <summary>
    ///     Role camp type.
    /// </summary>
    public CampType CampType => _metadata.CampType;

    /// <summary>
    ///     Vanilla role type template.
    /// </summary>
    public RoleTypes BaseRoleType
    {
        get => _metadata.BaseRoleType;
        protected set => _metadata.BaseRoleType = value;
    }

    /// <summary>
    ///     Whether this is a sub-role.
    /// </summary>
    public bool IsSubRole => _metadata.IsSubRole;

    /// <summary>
    ///     Whether to show this role in options.
    /// </summary>
    public bool ShowInOptions => _metadata.ShowInOptions;

    // ==================== Capabilities Delegates ====================

    /// <summary>
    ///     Whether the role can use vents.
    /// </summary>
    public bool CanVent
    {
        get => _capabilities.CanVent;
        protected init => _capabilities.CanVent = value;
    }

    /// <summary>
    ///     Whether the role can kill.
    /// </summary>
    public bool CanKill
    {
        get => _capabilities.CanKill;
        protected init => _capabilities.CanKill = value;
    }

    /// <summary>
    ///     Whether the role can use sabotage.
    /// </summary>
    public bool CanSabotage
    {
        get => _capabilities.CanSabotage;
        protected init => _capabilities.CanSabotage = value;
    }

    // ==================== Options Delegates ====================

    /// <summary>
    ///     Option for role count configuration.
    /// </summary>
    public CustomOption? RoleNumberOption
    {
        get => _options.RoleNumberOption;
        internal set => _options.RoleNumberOption = value;
    }

    /// <summary>
    ///     Option for role chance configuration.
    /// </summary>
    public CustomOption? RoleChanceOption
    {
        get => _options.RoleChanceOption;
        internal set => _options.RoleChanceOption = value;
    }

    /// <summary>
    ///     Option for role code configuration.
    /// </summary>
    public CustomOption? RoleCode
    {
        get => _options.RoleCode;
        internal set => _options.RoleCode = value;
    }

    /// <summary>
    ///     Whether this role is enabled (has count > 0).
    /// </summary>
    public bool Enabled => _options.Enabled;

    public LanguageConfig.TextHandler ActionNameContext { get; }

    /// <summary>
    ///     WARNING: Only local player performs this.
    /// </summary>
    public static Action<CustomRole, CustomButton> OnRoleAbilityUsed { get; set; } = (_, button) =>
    {
        EventRecorder.Instance.RpcRecord<UseAbilityGameEvent, UseAbilityEventSender>(
            new UseAbilityGameEvent(PlayerControl.LocalPlayer.GetPlayerData(), button));
    };

    public ReadOnlyCollection<PlayerControl> Players =>
        new(GameUtils.PlayerData.Where(pr => !pr.IsDisconnected && pr.Player.IsRole(this)).Select(pr => pr.Player)
            .ToList());

    /// <summary>
    ///     All options associated with this role.
    /// </summary>
    public List<CustomOption> AllOptions
    {
        get => _options.AllOptions;
        internal set => throw new NotSupportedException("AllOptions is managed by RoleOptions component.");
    }

    /// <summary>
    ///     Role options excluding count and chance options.
    /// </summary>
    public ReadOnlyCollection<CustomOption> RoleOptions => _options.RoleOptionsList;

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

    // ==================== Buttons Delegates ====================

    /// <summary>
    ///     The default kill button setting for this role.
    /// </summary>
    public KillButtonSetting DefaultKillButtonSetting => _buttons.DefaultKillButtonSetting;

    /// <summary>
    ///     The current active kill button setting.
    ///     Set to null to pop the previous setting from the stack.
    /// </summary>
    public KillButtonSetting CurrentKillButtonSetting
    {
        get => _buttons.CurrentKillButtonSetting;
        set => _buttons.CurrentKillButtonSetting = value;
    }

    /// <summary>
    ///     All custom buttons registered to this role.
    /// </summary>
    public List<CustomButton> AllButtons => _buttons.AllButtons;

    // ==================== Core Methods ====================

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
        return toReturn ?? LanguageConfig.Instance.GetString("role.global.no-details");
    }

    /// <summary>
    ///     Detailed description displayed in the role settings.
    /// </summary>
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
        _options.AllOptions.Add(option);

        return option;
    }

    protected void RegisterCustomOption(CustomOption option)
    {
        _options.AllOptions.Add(option.Register());
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
    ///     Adds a custom button to this role.
    /// </summary>
    /// <param name="button">The button to add.</param>
    /// <param name="hasButton">Optional condition to show the button.</param>
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

    // ==================== Eject & Name Handling ====================

    public virtual string HandleEjectText(NetworkedPlayerInfo player)
    {
        var role = player.GetMainRole();
        var sb = new StringBuilder(role.GetColorName());

        foreach (var subRole in player.GetSubRoles())
            sb.Append(' ').Append(subRole.GetColorName());

        return LanguageConfig.Instance.GetString("game.exile.default").CustomFormat(player.PlayerName, sb.ToString());
    }

    public virtual string HandleAdditionalPlayerName(PlayerControl player)
    {
        return "";
    }

    // ==================== Lifecycle Methods ====================

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

    /// <summary>
    ///     Legacy hook — receives raw <see cref="KnownRpc"/> packets routed by
    ///     <see cref="COG.Listener.Impl.RpcListener"/>.
    ///     <para/>
    ///     For new role-specific RPCs, use <see cref="NewRpc(int)"/> instead.
    ///     Prefer <see cref="OnRoleRpcReceived"/> for role-scoped manual handling.
    /// </summary>
    public virtual void OnRpcReceived(PlayerControl sender, byte callId, MessageReader reader)
    {
    }

    public virtual void OnUpdate()
    {
    }

    // ==================== RPC Handling ====================

    public void SyncRoleGameData()
    {
        var writer = RpcWriter.Start(KnownRpc.SyncRoleGameData).WritePacked(Id);
        OnRoleGameDataSynchronizing(writer);
        writer.Finish();
    }

    public string GetColorName()
    {
        return _metadata.GetColorName();
    }

    public void RegisterRpcHandler(IRpcHandler handler)
    {
        IRpcHandler.Register(handler);
    }

    protected RoleRpc NewRpc(Enum localId) => NewRpc(Convert.ToInt32(localId));

    protected RoleRpc NewRpc(int localId)
    {
        var rpc = new RoleRpc(this);
        RoleRpcManager.Register(this, localId, rpc);
        _roleRpcHandlers[rpc.AllocatedId] = rpc;
        return rpc;
    }

    protected RoleRpc<T> CreateRoleRpc<T>(Enum localId, Action<T> onPerform)
        where T : notnull
        => CreateRoleRpc<T>(Convert.ToInt32(localId), onPerform);

    protected RoleRpc<T> CreateRoleRpc<T>(int localId, Action<T> onPerform)
        where T : notnull
    {
        var rpc = new RoleRpc<T>(this, onPerform);
        RoleRpcManager.Register(this, localId, rpc);
        _roleRpcHandlers[rpc.AllocatedId] = rpc;
        return rpc;
    }

    protected RoleRpc<T> CreateRoleRpc<T>(
        Enum localId,
        Action<T> onPerform,
        Action<RpcWriter, T> onSerialize,
        Func<MessageReader, T> onDeserialize)
        where T : notnull
        => CreateRoleRpc<T>(Convert.ToInt32(localId), onPerform, onSerialize, onDeserialize);

    protected RoleRpc<T> CreateRoleRpc<T>(
        int localId,
        Action<T> onPerform,
        Action<RpcWriter, T> onSerialize,
        Func<MessageReader, T> onDeserialize)
        where T : notnull
    {
        var rpc = new RoleRpc<T>(this, onPerform, onSerialize, onDeserialize);
        RoleRpcManager.Register(this, localId, rpc);
        _roleRpcHandlers[rpc.AllocatedId] = rpc;
        return rpc;
    }

    protected RoleRpc<T1, T2> CreateRoleRpc<T1, T2>(Enum localId, Action<T1, T2> onPerform)
        where T1 : notnull where T2 : notnull
        => CreateRoleRpc<T1, T2>(Convert.ToInt32(localId), onPerform);

    protected RoleRpc<T1, T2> CreateRoleRpc<T1, T2>(int localId, Action<T1, T2> onPerform)
        where T1 : notnull where T2 : notnull
    {
        var rpc = new RoleRpc<T1, T2>(this, onPerform);
        RoleRpcManager.Register(this, localId, rpc);
        _roleRpcHandlers[rpc.AllocatedId] = rpc;
        return rpc;
    }

    protected RoleRpc<T1, T2> CreateRoleRpc<T1, T2>(
        Enum localId,
        Action<T1, T2> onPerform,
        Action<RpcWriter, T1, T2> onSerialize,
        Func<MessageReader, (T1, T2)> onDeserialize)
        where T1 : notnull where T2 : notnull
        => CreateRoleRpc<T1, T2>(Convert.ToInt32(localId), onPerform, onSerialize, onDeserialize);

    protected RoleRpc<T1, T2> CreateRoleRpc<T1, T2>(
        int localId,
        Action<T1, T2> onPerform,
        Action<RpcWriter, T1, T2> onSerialize,
        Func<MessageReader, (T1, T2)> onDeserialize)
        where T1 : notnull where T2 : notnull
    {
        var rpc = new RoleRpc<T1, T2>(this, onPerform, onSerialize, onDeserialize);
        RoleRpcManager.Register(this, localId, rpc);
        _roleRpcHandlers[rpc.AllocatedId] = rpc;
        return rpc;
    }

    protected RoleRpc<T1, T2, T3> CreateRoleRpc<T1, T2, T3>(Enum localId, Action<T1, T2, T3> onPerform)
        where T1 : notnull where T2 : notnull where T3 : notnull
        => CreateRoleRpc<T1, T2, T3>(Convert.ToInt32(localId), onPerform);

    protected RoleRpc<T1, T2, T3> CreateRoleRpc<T1, T2, T3>(int localId, Action<T1, T2, T3> onPerform)
        where T1 : notnull where T2 : notnull where T3 : notnull
    {
        var rpc = new RoleRpc<T1, T2, T3>(this, onPerform);
        RoleRpcManager.Register(this, localId, rpc);
        _roleRpcHandlers[rpc.AllocatedId] = rpc;
        return rpc;
    }

    protected RoleRpc<T1, T2, T3> CreateRoleRpc<T1, T2, T3>(
        Enum localId,
        Action<T1, T2, T3> onPerform,
        Action<RpcWriter, T1, T2, T3> onSerialize,
        Func<MessageReader, (T1, T2, T3)> onDeserialize)
        where T1 : notnull where T2 : notnull where T3 : notnull
        => CreateRoleRpc<T1, T2, T3>(Convert.ToInt32(localId), onPerform, onSerialize, onDeserialize);

    protected RoleRpc<T1, T2, T3> CreateRoleRpc<T1, T2, T3>(
        int localId,
        Action<T1, T2, T3> onPerform,
        Action<RpcWriter, T1, T2, T3> onSerialize,
        Func<MessageReader, (T1, T2, T3)> onDeserialize)
        where T1 : notnull where T2 : notnull where T3 : notnull
    {
        var rpc = new RoleRpc<T1, T2, T3>(this, onPerform, onSerialize, onDeserialize);
        RoleRpcManager.Register(this, localId, rpc);
        _roleRpcHandlers[rpc.AllocatedId] = rpc;
        return rpc;
    }

    internal void DispatchRoleRpc(IRoleRpc handler, PlayerControl sender, MessageReader reader)
    {
        if (_roleRpcHandlers.TryGetValue(handler.AllocatedId, out var registeredHandler))
        {
            registeredHandler.InvokeReceive(reader);
        }
        else
        {
            OnRoleRpcReceived(sender, handler.AllocatedId, reader);
        }
    }

    protected virtual void OnRoleRpcReceived(PlayerControl sender, uint allocatedId, MessageReader reader)
    {
    }

    // ==================== Kill Button Management ====================

    public void ResetCurrentKillButtonSetting()
    {
        CurrentKillButtonSetting = null!;
    }

    public static void ClearKillButtonSettings()
    {
        CustomRoleManager.GetManager().GetRoles().ForEach(r =>
        {
            r._buttons.ClearSettings();
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
