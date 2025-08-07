using System.Collections.Generic;

namespace COG.UI.Hud.CustomButton;

public class CustomButtonManager
{
    private static readonly CustomButtonManager Manager = new();
    private readonly List<CustomButton> _buttons = new();

    public static CustomButtonManager GetManager()
    {
        return Manager;
    }

    public void RegisterCustomButton(CustomButton button)
    {
        _buttons.Add(button);
    }

    public void RegisterCustomButtons(IEnumerable<CustomButton> button)
    {
        _buttons.AddRange(button);
    }

    public List<CustomButton> GetButtons()
    {
        return _buttons;
    }
}