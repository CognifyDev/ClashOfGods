namespace COG.Listener.Event.Impl;

public class AmongUsClientEvent : Event
{
    public AmongUsClientEvent(AmongUsClient client)
    {
        AmongUsClient = client;
    }

    public AmongUsClient AmongUsClient { get; }
}