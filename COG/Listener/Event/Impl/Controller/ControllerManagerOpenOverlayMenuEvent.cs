namespace COG.Listener.Event.Impl.Controller;

public class ControllerManagerOpenOverlayMenuEvent : ControllerEvent
{
    public ControllerManagerOpenOverlayMenuEvent(ControllerManager manager,
        string menuName,
        UiElement backButton,
        UiElement defaultSelection,
        UiElement[] selectableElements,
        bool gridNav) : base(manager)
    {
        MenuName = menuName;
        BackButton = backButton;
        DefaultSelection = defaultSelection;
        SelectableElements = selectableElements;
        GridNav = gridNav;
    }
    
    public string MenuName { get; }
    public UiElement BackButton { get; }
    public UiElement DefaultSelection { get; }
    public UiElement[] SelectableElements { get; }
    public bool GridNav { get; }
}