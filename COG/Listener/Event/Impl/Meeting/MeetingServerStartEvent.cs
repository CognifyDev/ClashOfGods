﻿namespace COG.Listener.Event.Impl.Meeting;

/// <summary>
/// Postfix Only.
/// Only host can handle.
/// </summary>
public class MeetingServerStartEvent : MeetingEvent
{
    public byte Reporter { get; }
    
    public MeetingServerStartEvent(MeetingHud meeting, byte reporter) : base(meeting)
    {
        Reporter = reporter;
    }
}