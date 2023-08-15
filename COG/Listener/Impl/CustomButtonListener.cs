using COG.UI.CustomButton;
using COG.UI.CustomButtons;

namespace COG.Listener.Impl;

internal class CustomButtonListener : IListener
{
    public void OnHudStart(HudManager hud)
    {
        CustomButton.Init(hud);
    }

    public void OnHudUpdate(HudManager manager)
    {
        foreach (var button in CustomButtonManager.GetManager().GetButtons()) button.Update();
    }
}