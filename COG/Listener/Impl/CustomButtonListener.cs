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
        // Only invalidate Unity-side references; keep button registrations alive
        // so they survive across game rounds (roles re-add via constructor).
        foreach (var btn in CustomButtonManager.GetManager().GetButtons())
        {
            btn.ButtonObject = null;
            btn.GameObject = null;
            btn.SpriteRenderer = null;
            btn.Material = null;
            btn.PassiveButton = null;
            btn.TextMesh = null;
            btn.HotkeyRenderer = null;
            btn.HotkeyText = null;
            btn.InfoText = null;
        }
    }
}