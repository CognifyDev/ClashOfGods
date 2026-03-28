using System.Linq;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Attribute;
using COG.Listener.Event.Impl.Meeting;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.Meeting;
using COG.Utils;
using TMPro;
using UnityEngine;

namespace COG.Role.Impl.SubRole;

public class Guesser : CustomRole, IListener, IMeetingButton
{    
    private TextMeshPro? _remainingGuessText;

    public NetworkedPlayerInfo? CurrentGuessing { get; set; }

    public int GuessedTime { get; internal set; }

    public CustomOption MaxGuessTime { get; }
    public CustomOption GuessContinuously { get; }
    public CustomOption EnabledRolesOnly { get; }

    public Sprite MeetingButtonSprite =>
        ResourceUtils.LoadSprite(ResourceConstant.GuessButton, 150f);

    public bool ShouldShowMeetingButton() =>
        PlayerControl.LocalPlayer.IsAlive() &&
        GuessedTime < MaxGuessTime.GetInt();

    public bool ShouldShowMeetingButtonFor(PlayerVoteArea pva)
    {
        if (pva.AmDead) return false;
        if (pva.TargetPlayerId == PlayerControl.LocalPlayer.PlayerId &&
            !GlobalCustomOptionConstant.DebugMode.GetBool())
            return false;

        var data = GameUtils.PlayerData.FirstOrDefault(pd => pd.PlayerId == pva.TargetPlayerId);
        return data != null && !data.IsDisconnected;
    }

    public void OnMeetingButtonClick(PlayerVoteArea pva)
    {
        var target = GuesserButton.Buttons.FirstOrDefault(b => b.Area == pva);
        target?.OpenGuessUI(MeetingHud.Instance);
    }

    public void OnMeetingButtonUpdate(MeetingHud meetingHud)
    {
        if (CurrentGuessing == null) return;

        var data = GameUtils.PlayerData.FirstOrDefault(pd => CurrentGuessing.PlayerId == pd.PlayerId);
        if (data != null && data.IsDisconnected)
            GuesserButton.Buttons.ForEach(b => b.CloseGuessUI());
    }

    public Guesser() : base(ColorUtils.FromColor32(192, 0, 0))
    {
        MaxGuessTime = CreateOption(
            () => LanguageConfig.Instance.GuesserMaxGuessTime,
            new FloatOptionValueRule(1, 1, 15, 3));

        GuessContinuously = CreateOption(
            () => LanguageConfig.Instance.GuesserGuessContinuously,
            new BoolOptionValueRule(true));

        EnabledRolesOnly = CreateOption(
            () => LanguageConfig.Instance.GuesserGuessEnabledRolesOnly,
            new BoolOptionValueRule(true));
    }

    [EventHandler(EventHandlerType.Postfix)]
    [OnlyLocalPlayerWithThisRoleInvokable]
    public void OnMeetingStart(MeetingStartEvent @event)
    {
        var player = PlayerControl.LocalPlayer;
        if (!player.IsAlive()) return;

        var meetingHud = @event.MeetingHud;

        _remainingGuessText = Object.Instantiate(meetingHud.TitleText, meetingHud.transform);
        _remainingGuessText.TryDestroyComponent<TextTranslatorTMP>();
        _remainingGuessText.transform.localPosition = new Vector3(-2.9f, 2.2f, -1f);
        _remainingGuessText.name = "RemainingGuessInfo";
        UpdateRemainingText();

        if (GuessedTime >= MaxGuessTime.GetInt()) return;

        var guessableRoles = (EnabledRolesOnly.GetBool()
            ? GetCustomRolesFromPlayers()
            : CustomRoleManager.GetManager().GetModRoles().Where(r => !r.IsSubRole))
            .ToList();

        for (var i = 0; i < meetingHud.playerStates.Count; i++)
        {
            var pva = meetingHud.playerStates[i];
            if (pva == null) continue;
            if (!ShouldShowMeetingButtonFor(pva)) continue;

            new GuesserButton(
                    pva.Buttons.transform.Find("CancelButton").gameObject,
                    pva, this, meetingHud, guessableRoles)
                .Register();
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnVotingComplete(MeetingVotingCompleteEvent @event)
    {
        GuesserButton.DestroyAll();
    }

    public override void OnUpdate()
    {
        if (_remainingGuessText) UpdateRemainingText();
        if (CurrentGuessing != null)
        {
            var data = GameUtils.PlayerData.FirstOrDefault(pd => CurrentGuessing.PlayerId == pd.PlayerId);
            if (data != null && data.IsDisconnected)
                GuesserButton.Buttons.ForEach(b => b.CloseGuessUI());
        }
    }

    public override IListener GetListener() => this;

    private void UpdateRemainingText()
    {
        _remainingGuessText!.text = LanguageConfig.Instance
            .GetHandler("role.sub-roles.guesser.in-game")
            .GetString("remaining-guesses")
            .CustomFormat(MaxGuessTime.GetInt() - GuessedTime);
    }

    private CustomRole[] GetCustomRolesFromPlayers()
    {
        return PlayerUtils.GetAllPlayers()
            .Select(p => p.GetMainRole())
            .ToArray();
    }
}
