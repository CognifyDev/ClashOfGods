using System.Collections.Generic;
using COG.UI.Hud.CustomButton;
using COG.UI.Vanilla.KillButton;
using UnityEngine;

namespace COG.Role.Components;

/// <summary>
///     Manages button state and custom buttons for a role.
/// </summary>
public class RoleButtons
{
    private readonly Stack<KillButtonSetting> _killButtonSettings = new();

    /// <summary>
    ///     All custom buttons registered to this role.
    /// </summary>
    public List<CustomButton> AllButtons { get; } = new();

    /// <summary>
    ///     The default kill button setting for this role.
    /// </summary>
    public KillButtonSetting DefaultKillButtonSetting { get; }

    private KillButtonSetting _currentKillButtonSetting;

    /// <summary>
    ///     The current active kill button setting.
    ///     Set to null to pop the previous setting from the stack.
    /// </summary>
    public KillButtonSetting CurrentKillButtonSetting
    {
        get => _currentKillButtonSetting;
        set
        {
            if (value == null!)
            {
                if (_killButtonSettings.Count > 0)
                    _currentKillButtonSetting = _killButtonSettings.Pop();
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

    public RoleButtons()
    {
        DefaultKillButtonSetting = new KillButtonSetting
        {
            ForceShow = () => false,
            TargetOutlineColor = Color.clear
        };
        _currentKillButtonSetting = DefaultKillButtonSetting;
    }

    /// <summary>
    ///     Resets the current kill button setting to the default.
    /// </summary>
    public void ResetCurrentKillButtonSetting()
    {
        CurrentKillButtonSetting = null!;
    }

    /// <summary>
    ///     Clears all kill button settings and resets to default.
    /// </summary>
    public void ClearSettings()
    {
        _killButtonSettings.Clear();
        ResetCurrentKillButtonSetting();
    }
}
