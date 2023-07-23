using System.Collections.Generic;

namespace COG.UI.CustomButtons;

public class CustomButtonManager
{
    private static CustomButtonManager _manager = new();
    private readonly List<CustomButton> Buttons = new();

    public static CustomButtonManager GetManager() => _manager;
    
    public void RegisterCustomButton(CustomButton option)
    {
        Buttons.Add(option);
    }

    public void RegisterCustomButtons(CustomButton[] options)
    {
        Buttons.AddRange(options);
    }

    public List<CustomButton> GetButtons() => Buttons;
}