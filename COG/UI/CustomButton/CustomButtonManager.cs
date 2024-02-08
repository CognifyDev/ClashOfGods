using System.Collections.Generic;

namespace COG.UI.CustomButton;

public class CustomButtonManager
{
    private static readonly CustomButtonManager Manager = new();
    private readonly List<CustomButton.CustomButton> _buttons = new();

    public static CustomButtonManager GetManager()
    {
        return Manager;
    }

    public void RegisterCustomButton(CustomButton.CustomButton button)
    {
        _buttons.Add(button);
    }

    public void RegisterCustomButtons(IEnumerable<CustomButton.CustomButton> button)
    {
        _buttons.AddRange(button);
    }

    public List<CustomButton.CustomButton> GetButtons()
    {
        return _buttons;
    }
}
