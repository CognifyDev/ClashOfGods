using System;
using COG.UI.Hud.CustomButton;
using UnityEngine;

namespace COG.Role.Options;

/// <summary>
/// Simplified button builder that automatically registers the button with the role.
/// </summary>
public class RoleButtonBuilder
{
    private readonly CustomRole _role;
    private readonly CustomButton.CustomButtonBuilder _builder;
    
    public RoleButtonBuilder(CustomRole role, string key, Sprite icon, string text)
    {
        _role = role;
        _builder = CustomButton.Builder(key, icon, text);
    }
    
    public RoleButtonBuilder OnClick(Action onClick) { _builder.OnClick(onClick); return this; }
    public RoleButtonBuilder OnEffect(float effectTime, Action onEffect) { _builder.OnEffect(effectTime, onEffect); return this; }
    public RoleButtonBuilder CouldUse(Func<bool> condition) { _builder.CouldUse(condition); return this; }
    public RoleButtonBuilder HasButton(Func<bool> condition) { _builder.HasButton(condition); return this; }
    public RoleButtonBuilder Cooldown(Func<float> cooldown) { _builder.Cooldown(cooldown); return this; }
    public RoleButtonBuilder UsesLimit(int limit) { _builder.UsesLimit(limit); return this; }
    public RoleButtonBuilder OnMeetingEnds(Action onEnd) { _builder.OnMeetingEnds(onEnd); return this; }
    public RoleButtonBuilder Position(Vector3 position) { _builder.Position(position); return this; }
    public RoleButtonBuilder Order(int order) { _builder.Order(order); return this; }
    public RoleButtonBuilder Row(int row) { _builder.Row(row); return this; }
    
    /// <summary>
    /// Build the button and automatically register it with the role.
    /// </summary>
    public CustomButton Build()
    {
        var btn = _builder.Build();
        _role.AddButton(btn);
        return btn;
    }
}
