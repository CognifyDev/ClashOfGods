namespace COG.Listener.Event.Impl.Meeting;

/// <summary>
///     当一个玩家投票的时候，会触发这个事件
/// </summary>
public class MeetingCastVoteEvent : MeetingEvent
{
    public MeetingCastVoteEvent(MeetingHud meetingHud, PlayerControl voter, PlayerControl? target, bool didSkip) :
        base(meetingHud)
    {
        Voter = voter;
        Target = target;
        IsSkipped = didSkip;
    }

    /// <summary>
    ///     投票者
    /// </summary>
    public PlayerControl Voter { get; }

    /// <summary>
    ///     被投票者
    ///     当是跳过的时候或者退出的时候这个会返回 null
    /// </summary>
    public PlayerControl? Target { get; }

    /// <summary>
    ///     是否是跳过投票
    /// </summary>
    public bool IsSkipped { get; }
}