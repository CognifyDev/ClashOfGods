using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules;

namespace COG.Role.Components;

/// <summary>
///     Manages role configuration options.
/// </summary>
public class RoleOptions
{
    /// <summary>
    ///     All options associated with this role.
    /// </summary>
    public List<CustomOption> AllOptions { get; } = new();

    /// <summary>
    ///     Option for role count configuration.
    /// </summary>
    public CustomOption? RoleNumberOption { get; internal set; }

    /// <summary>
    ///     Option for role chance configuration.
    /// </summary>
    public CustomOption? RoleChanceOption { get; internal set; }

    /// <summary>
    ///     Option for role code configuration.
    /// </summary>
    public CustomOption? RoleCode { get; internal set; }

    /// <summary>
    ///     Whether this role is enabled (has count > 0).
    /// </summary>
    public bool Enabled => RoleNumberOption?.GetInt() > 0;

    /// <summary>
    ///     Role options excluding count and chance options.
    /// </summary>
    public ReadOnlyCollection<CustomOption> RoleOptionsList =>
        new(AllOptions.FindAll(o => o != RoleNumberOption && o != RoleChanceOption));

    /// <summary>
    ///     Creates and registers a new option for this role.
    /// </summary>
    public CustomOption CreateOption(CustomOption.TabType tabType, Func<string> nameGetter, IValueRule rule)
    {
        var option = CustomOption.Of(tabType, nameGetter, rule).Register();
        AllOptions.Add(option);
        return option;
    }

    /// <summary>
    ///     Registers an existing option to this role.
    /// </summary>
    public void RegisterOption(CustomOption option)
    {
        AllOptions.Add(option.Register());
    }
}
