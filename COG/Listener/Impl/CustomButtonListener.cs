using System.Threading;
using COG.Listener.Event.Impl.HManager;
using COG.UI.CustomButton;

namespace COG.Listener.Impl;

internal class CustomButtonListener : IListener
{
    [EventHandler(EventHandlerType.Postfix)]
    public void OnHudStart(HudManagerStartEvent @event)
    {
        CustomButton.Initialized = false;
        CustomButton.Init(@event.Manager);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnHudUpdate(HudManagerUpdateEvent @event)
    {
        if (!CustomButton.Initialized) return;
        CustomButton.ArrangeAllVanillaButtons();
        CustomButton.ArrangePosition();
        foreach (var button in CustomButtonManager.GetManager().GetButtons()) button.Update();
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnHudDestroy(HudManagerDestroyEvent @event)
    {
        CustomButton.Initialized = false;
    }
}