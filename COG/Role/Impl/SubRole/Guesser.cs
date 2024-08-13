using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.Meeting;
using COG.UI.CustomGameObject.Meeting;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.SubRole;

public class Guesser : CustomRole, IListener
{
    public CustomOption MaxGuessTime { get; }
    public CustomOption GuessContinuously { get; }
    public CustomOption EnabledRolesOnly { get; }
    public CustomOption CanGuessSubRoles { get; }

    public int HaveGuessedTime { get; } = 0;

    public bool HasGuessed { get; private set; } 
    
    public Guesser() : base(new Color(192, 0, 0, 100), CampType.Unknown, true)
    {
        MaxGuessTime = CreateOption(() => LanguageConfig.Instance.GuesserMaxGuessTime,
            new IntOptionValueRule(1, 1, 15, 3));
        GuessContinuously = CreateOption(() => LanguageConfig.Instance.GuesserGuessContinuously, 
            new BoolOptionValueRule(true));
        EnabledRolesOnly = CreateOption(() => LanguageConfig.Instance.GuesserGuessEnabledRolesOnly,
            new BoolOptionValueRule(true));
        CanGuessSubRoles = CreateOption(() => LanguageConfig.Instance.GuesserCanGuessSubRoles,
            new BoolOptionValueRule(false));
    }
    
    [EventHandler(EventHandlerType.Postfix)]
    public void AfterMeetingHudServerStart(MeetingServerStartEvent @event)
    {
        var player = PlayerControl.LocalPlayer;
        if (!player.IsRole(this) || !player.IsAlive()) return;
        HasGuessed = false;
        var meetingHud = @event.MeetingHud;
        var debugEnabled = GlobalCustomOptionConstant.DebugMode.GetBool();
        for (var i = 0; i < meetingHud.playerStates.Count; i++)
        {
            var playerVoteArea = meetingHud.playerStates[i];
            
            // 如果死了或者是当前玩家则不要布置
            if ((playerVoteArea == null || playerVoteArea.AmDead || playerVoteArea.TargetPlayerId == player.PlayerId) &&
                !debugEnabled) continue;
            var guessButton = new GuessButton(meetingHud, playerVoteArea!.Buttons.transform.Find("CancelButton").gameObject,
                playerVoteArea, this);
            
            guessButton.SetupListeners();
        }
    }

    public override CustomRole NewInstance()
    {
        return new Guesser();
    }

    public override IListener GetListener()
    {
        return this;
    }
}