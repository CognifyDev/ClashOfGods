using System.Collections.Generic;

namespace COG.UI.ModOption;

public class ModOption
{
    public readonly string Text;
    public readonly System.Func<bool> OnClick;
    public readonly bool DefaultValue;

    public ToggleButtonBehaviour? ToggleButton;

    public static readonly List<ModOption> Buttons = new();

    public ModOption(string text, System.Func<bool> onClick, bool defaultValue)
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