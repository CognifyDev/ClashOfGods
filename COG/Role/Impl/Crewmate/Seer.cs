using System.Collections.Generic;
using System.Linq;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Attribute;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Event.Impl.Modded.Player;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.Utils;

namespace COG.Role.Impl.Crewmate;

public class Seer : COG.Role.Camp.CrewmateRole
{
    private CustomButton CheckButton { get; }

    private int AvailableUsageTimes { get; set; }

    private CustomOption Cooldown { get; }

    private CustomOption InitialAvailableUsableTimes { get; }

    private readonly Dictionary<byte, string> _prefixes = new();

    private PlayerControl? _current;

    private readonly List<PlayerControl> _checkedPlayers = [];

    public Seer() : base(ColorUtils.FromColor32(30,144,255))
    {
        Cooldown = CreateOption(() =>
                GetContextFromLanguage("check-cooldown"),
            new FloatOptionValueRule(5, 1, 60, 25, NumberSuffixes.Seconds));

        InitialAvailableUsableTimes = CreateOption(() =>
                GetContextFromLanguage("initial-available-usable-times"),
            new FloatOptionValueRule(1, 1, 15, 1));

        var action = new LanguageConfig.TextHandler("action");

        CheckButton = CustomButton.Builder("seer-check",
                ResourceConstant.CheckButton, action.GetString("check"))
            .OnClick(() =>
            {
                if (_current == null) return;

                _checkedPlayers.Add(_current);
                ShowCurrentCamp(_current);
                AvailableUsageTimes --;
            })
            .CouldUse(() =>
            {
                PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out var target);
                _current = target;
                return AvailableUsageTimes !<= 0 && !HasChecked(_current) && _current != null;
            })
            .Cooldown(Cooldown.GetFloat)
            .Build();

        AddButton(CheckButton);

        On<PlayerMurderEvent>(OnPlayerMurder);
        On<PlayerCustomRoleChangeEvent>(OnPlayerRoleChange);
        On<GameStartEvent>(OnGameStart);
    }

    private bool HasChecked(PlayerControl? target)
    {
        return target != null && _checkedPlayers.Any(current => current.PlayerId == target.PlayerId);
    }

    [LocalOnly]
    private void OnPlayerMurder(PlayerMurderEvent @event)
    {
        if (MurderResultFlags.Succeeded != @event.MurderResult!.Value)
            return;
        if (!HasChecked(@event.Target))
            return;
        else _checkedPlayers.Remove(@event.Target);

        AvailableUsageTimes ++;
    }

    [LocalOnly]
    private void OnPlayerRoleChange(PlayerCustomRoleChangeEvent @event)
    {
        if (@event.OriginRole.Equals(@event.TargetRole))
            return;
        if (!HasChecked(@event.Player))
            return;

        ShowCurrentCamp(@event.Player);
    }

    [LocalOnly]
    private void OnGameStart(GameStartEvent _)
    {
        AvailableUsageTimes = (int)InitialAvailableUsableTimes.GetFloat();
        PlayerUtils.GetAllPlayers().ForEach(target => _prefixes.Add(target.PlayerId, target.Data.PlayerName));
    }

    private void ShowCurrentCamp(PlayerControl target)
    {
        if (_prefixes.TryGetValue(target.PlayerId, out var prefix))
        {
            target.Data.PlayerName = prefix + $"({target.GetMainRole().CampType.GetName()})";
        }
    }

    public override void ClearRoleGameData()
    {
        // Restore modified player names before clearing
        foreach (var (playerId, originalName) in _prefixes)
        {
            var player = PlayerUtils.GetPlayerById(playerId);
            if (player?.Data != null)
                player.Data.PlayerName = originalName;
        }
        _checkedPlayers.Clear();
        _prefixes.Clear();
    }
}