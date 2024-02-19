namespace COG.Listener;

/// <summary>
/// LISTENER INTERFACE
/// 
/// This is the interface used to mark a class
/// as a listener
/// </summary>
public interface IListener
{
    public static readonly IListener EmptyListener = new EmptyListenerClass();
    
    private class EmptyListenerClass : IListener { }
}