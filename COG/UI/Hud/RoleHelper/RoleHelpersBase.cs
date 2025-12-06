using TMPro;

namespace COG.UI.Hud.RoleHelper;

public class RoleHelpersBase
{
    public RoleHelpersBase(TextMeshPro title, TextMeshPro subtitle, TextMeshPro text, SecondTextType type)
    {
        Title = title;
        SubTtile = subtitle;
        Text = text;
        Type = type;
    }

    public TextMeshPro Title { get; set; }
    public TextMeshPro SubTtile { get; set; }
    public TextMeshPro Text { get; set; }
    public SecondTextType Type { get; set; }
}

public enum SecondTextType
{
    None,
    Option,
    Button,
    Special
}