using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Meeting;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils.Coding;
using UnityEngine;

namespace COG.Role.Impl.SubRole;

[NotUsed]
[WorkInProgress]
[NotTested]
public class Guesser : CustomRole, IListener
{
    public readonly CustomOption? GuesserGuessContinuously;
    public readonly CustomOption? MaxGuessTimeOption;

    public Guesser() : base(LanguageConfig.Instance.GuesserName, Color.yellow, CampType.Unknown)
    {
        Description = LanguageConfig.Instance.GuesserDescription;
        IsSubRole = true;

        MaxGuessTimeOption = CreateOption(() => LanguageConfig.Instance.GuesserMaxGuessTime,
            new FloatOptionValueRule(1, 1, 99, 5));
        GuesserGuessContinuously = CreateOption(() => LanguageConfig.Instance.GuesserGuessContinuously,
            new BoolOptionValueRule(true));
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnMeetingStart(MeetingStartEvent @event)
    {
        var buttonStuff = GameObject.Find("Main Camera/Hud/MeetingHub/MeetingContents/ButtonStuff/");
        // ui here
    }

    public override IListener GetListener()
    {
        return this;
    }

    public override CustomRole NewInstance()
    {
        return new Guesser();
    }
}