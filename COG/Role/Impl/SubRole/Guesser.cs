using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Meeting;
using COG.UI.CustomOption;
using COG.Utils.Coding;
using UnityEngine;

namespace COG.Role.Impl.SubRole;

[NotUsed]
[NotTested]
public class Guesser : CustomRole, IListener
{
    public readonly CustomOption? MaxGuessTimeOption;
    public readonly CustomOption? GuesserGuessContinuously;

    public Guesser() : base(LanguageConfig.Instance.GuesserName, Color.yellow, CampType.Unknown)
    {
        Description = LanguageConfig.Instance.GuesserDescription;
        IsSubRole = true;

        MaxGuessTimeOption = CustomOption.Create(CustomOption.TabType.Addons,
            LanguageConfig.Instance.GuesserMaxGuessTime, 5, 1, 99, 1, MainRoleOption);
        GuesserGuessContinuously = CustomOption.Create(CustomOption.TabType.Addons,
            LanguageConfig.Instance.GuesserGuessContinuously, true, MainRoleOption);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnMeetingStart(MeetingStartEvent @event)
    {
        var buttonStaff = GameObject.Find("Main Camera/Hud/MeetingHub/MeetingContents/ButtonStuff/");
        // ui here
    }

    public override IListener GetListener()
    {
        return this;
    }
}