using System;
using System.Collections.Generic;

namespace COG.UI.ClientOption;

public class ClientOption
{
    public static readonly List<ClientOption> Buttons = new();
    public readonly bool DefaultValue;
    public readonly Func<bool> OnClick;
    public readonly string Text;

    public ToggleButtonBehaviour? ToggleButton;

    public ClientOption(string text, Func<bool> onClick, bool defaultValue)
    {
        Text = text;
        OnClick = onClick;
        DefaultValue = defaultValue;
    }

    public void Register()
    {
        Buttons.Add(this);
    }
}