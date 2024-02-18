namespace COG.Listener.Event.Impl;

public class AmongUsClientEvent : Listener.Event.Event
{
    public AmongUsClient AmongUsClient { get; }

    public AmongUsClientEvent(AmongUsClient client)
    {
        AmongUsClient = client;
    }
}