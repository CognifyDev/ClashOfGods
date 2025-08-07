using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using COG.Listener.Event.Impl.HManager;
using COG.UI.Hud.CustomButton;
using COG.Utils;

namespace COG.Listener.Impl;

internal class CustomButtonListener : IListener
{
    [EventHandler(EventHandlerType.Postfix)]
    public void OnHudStart(HudManagerStartEvent @event)
    {
        CustomButton.Initialized = false;
        TaskUtils.RunTaskAfter(1, () => CustomButton.Init(@event.Manager));
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnHudUpdate(HudManagerUpdateEvent @event)
    {
        if (!CustomButton.Initialized) return;
        //CustomButton.ArrangePosition();
        foreach (var button in CustomButtonManager.GetManager().GetButtons()) button.Update();
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnHudDestroy(HudManagerDestroyEvent @event)
    {
        CustomButton.Initialized = false;
    }
}