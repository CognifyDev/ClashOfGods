using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Attribute;
using COG.Listener.Event.Impl.Meeting;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.Meeting;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.SubRole;

public class Guesser : CustomRole, IListener
{
    public Guesser() : base(ColorUtils.FromColor32(192, 0, 0))
    {
        MaxGuessTime = CreateOption(() => LanguageConfig.Instance.GuesserMaxGuessTime,
            new IntOptionValueRule(1, 1, 15, 3));
        GuessContinuously = CreateOption(() => LanguageConfig.Instance.GuesserGuessContinuously,
            new BoolOptionValueRule(true));
        EnabledRolesOnly = CreateOption(() => LanguageConfig.Instance.GuesserGuessEnabledRolesOnly,
            new BoolOptionValueRule(false));
        /*CanGuessSubRoles = CreateOption(() => LanguageConfig.Instance.GuesserCanGuessSubRoles,
            new BoolOptionValueRule(false));*/
    }

    public CustomOption MaxGuessTime { get; }
    public CustomOption GuessContinuously { get; }

    public CustomOption EnabledRolesOnly { get; }
    //public CustomOption CanGuessSubRoles { get; }

    public int GuessedTime { get; internal set; }

    [EventHandler(EventHandlerType.Postfix)]
    [OnlyLocalPlayerWithThisRoleInvokable]
    public void OnMeetingStart(MeetingStartEvent @event)
    {
        var player = PlayerControl.LocalPlayer;
        if (!player.IsAlive()) return;

        var meetingHud = @event.MeetingHud;

        var infoTemplate = meetingHud.TitleText;
        var infoText = Object.Instantiate(infoTemplate, meetingHud.transform);
        infoText.TryDestroyComponent<TextTranslatorTMP>();
        infoText.transform.localPosition = new Vector3(-2.9f, 2.2f, -1f);
        infoText.name = "RemainingGuessInfo";
        infoText.text = LanguageConfig.Instance.GetHandler("role.sub-roles.guesser.in-game")
            .GetString("remaining-guesses").CustomFormat(MaxGuessTime.GetInt() - GuessedTime);

        if (GuessedTime >= MaxGuessTime.GetInt()) return;

        // NO FOREACH
        for (var i = 0; i < meetingHud.playerStates.Count; i++)
        {
            var playerVoteArea = meetingHud.playerStates[i];

            // 如果死了或者是当前玩家则不要布置
            if (playerVoteArea == null || playerVoteArea.AmDead ||
                playerVoteArea.TargetPlayerId == player.PlayerId) continue;
            var guessButton = new GuesserButton(playerVoteArea!.Buttons.transform.Find("CancelButton").gameObject,
                playerVoteArea, this);
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnVotingComplete(MeetingVotingCompleteEvent @event)
    {
        GuesserButton.DestroyAll();
    }

    public override IListener GetListener()
    {
        return this;
    }
}