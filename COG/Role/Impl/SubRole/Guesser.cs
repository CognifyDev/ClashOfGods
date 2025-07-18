﻿using COG.Config.Impl;
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
    //public CustomOption CanGuessSubRoles { get; }

    public int GuessedTime { get; internal set; }
    
    public Guesser() : base(ColorUtils.FromColor32(192, 0, 0, 100))
    {
        MaxGuessTime = CreateOption(() => LanguageConfig.Instance.GuesserMaxGuessTime,
            new FloatOptionValueRule(1F, 1F, 15F, 3F));
        GuessContinuously = CreateOption(() => LanguageConfig.Instance.GuesserGuessContinuously, 
            new BoolOptionValueRule(true));
        EnabledRolesOnly = CreateOption(() => LanguageConfig.Instance.GuesserGuessEnabledRolesOnly,
            new BoolOptionValueRule(false));
        /*CanGuessSubRoles = CreateOption(() => LanguageConfig.Instance.GuesserCanGuessSubRoles,
            new BoolOptionValueRule(false));*/
    }
    
    [EventHandler(EventHandlerType.Postfix)]
    public void AfterMeetingHudServerStart(MeetingStartEvent @event)
    {
        var player = PlayerControl.LocalPlayer;
        if (!player.IsRole(this) || !player.IsAlive()) return;
        var meetingHud = @event.MeetingHud;
        var debugEnabled = GlobalCustomOptionConstant.DebugMode.GetBool();
        
        if (GuessedTime >= MaxGuessTime.GetFloat())
        {
            GuessButton.Buttons.ForEach(guessButton => guessButton.Destroy());
            GuessButton.Buttons.Clear();
        }
        
        GuessButton.Buttons.Clear();
        
        // NO FOREACH
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
    
    public override IListener GetListener()
    {
        return this;
    }
}