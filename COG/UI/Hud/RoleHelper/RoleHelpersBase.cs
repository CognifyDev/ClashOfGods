using TMPro;

namespace COG.UI.Hud.RoleHelper;

public class RoleHelpersBase
{
    public RoleHelpersBase(string title, string subtitle, string text, SecondTextType type)
    {
        Title = title;
        SubTtile = subtitle;
        Text = text;
        Type = type;
    }

    public string Title { get; set; }
    public string SubTtile { get; set; }
    public string Text { get; set; }
    public SecondTextType Type { get; set; }
}

public enum SecondTextType
{
    None,
    Option,
    Button,
    Special
}