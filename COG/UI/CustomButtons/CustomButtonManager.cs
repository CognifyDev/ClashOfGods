using System.Collections.Generic;

namespace COG.UI.CustomButtons;

public class CustomButtonManager
{
    private static CustomButtonManager _manager = new();
    private readonly List<CustomButton> Buttons = new();

    public static CustomButtonManager GetManager() => _manager;
    
    public void RegisterCustomButton(CustomButton button)
    {
        Buttons.Add(button);
    }

    public void RegisterCustomButtons(CustomButton[] button)
    {
        Buttons.AddRange(button);
    }

    public List<CustomButton> GetButtons() => Buttons;
}