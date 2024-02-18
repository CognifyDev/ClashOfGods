namespace COG.NewListener.Event.Impl;

public class AmongUsClientEvent : Event
{
    public AmongUsClient AmongUsClient { get; }

    public AmongUsClientEvent(AmongUsClient client)
    {
        AmongUsClient = client;
    }
}