using System.Collections.Generic;

namespace COG.UI.CustomButtons;

public class CustomButtonManager
{
    private static readonly CustomButtonManager Manager = new();
    private readonly List<CustomButton> _buttons = new();

    public static CustomButtonManager GetManager() => Manager;
    
    public void RegisterCustomButton(CustomButton button)
    {
        _buttons.Add(button);
    }

    public void RegisterCustomButtons(IEnumerable<CustomButton> button)
    {
        _buttons.AddRange(button);
    }

    public List<CustomButton> GetButtons() => _buttons;
}