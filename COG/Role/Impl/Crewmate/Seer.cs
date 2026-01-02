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
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Seer : CustomRole
{
    private CustomButton CheckButton { get; }

    private int AvailableUsageTimes { get; set; }

    private CustomOption Cooldown { get; }

    private CustomOption InitialAvailableUsableTimes { get; }

    private readonly Dictionary<byte, string> _prefixes = new();

    private PlayerControl? _current;

    private readonly HashSet<PlayerControl> _checkedPlayers = [];
    
    public Seer() : base(new Color(30,144,255), CampType.Crewmate)
    {
        Cooldown = CreateOption(() => 
                GetContextFromLanguage("role.crewmate.seer-check-cooldown"),
            new FloatOptionValueRule(5, 1, 60, 25, NumberSuffixes.Seconds));
        
        InitialAvailableUsableTimes = CreateOption(() => 
                GetContextFromLanguage("role.crewmate.seer-initial-available-usable-times"),
            new FloatOptionValueRule(1, 1, 15, 1));

        var action = new LanguageConfig.TextHandler("action");

        CheckButton = CustomButton.Of(
            "seer-check",
            () =>
            {
                if (_current == null) return;

                _checkedPlayers.Add(_current);
                ShowCurrentCamp(_current);
                AvailableUsageTimes --;
            }, 
            () => { },
            () => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _current) && AvailableUsageTimes > 0 && !HasChecked(_current),
            () => true,
            ResourceUtils.LoadSprite(ResourceConstant.CleanDeadBodyButton),
            2,
            action.GetString("check"),
            () => Cooldown.GetFloat(),
            -1
        );
 
        AddButton(CheckButton);
    }

    private bool HasChecked(PlayerControl? target)
    {
        return target != null && _checkedPlayers.Any(current => current.PlayerId == target.PlayerId);
    }
    
    [OnlyLocalPlayerWithThisRoleInvokable]
    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerMurder(PlayerMurderEvent @event)
    {
        if (MurderResultFlags.Succeeded != @event.MurderResult!.Value)
            return;
        if (!HasChecked(@event.Target))
            return;

        _checkedPlayers.Remove(@event.Target);
        AvailableUsageTimes ++;
    }

    [OnlyLocalPlayerWithThisRoleInvokable]
    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerRoleChange(PlayerCustomRoleChangeEvent @event)
    {
        if (@event.OriginRole.Equals(@event.TargetRole))
            return;
        if (!HasChecked(@event.Player))
            return;
        
        ShowCurrentCamp(@event.Player);
    }

    [OnlyLocalPlayerWithThisRoleInvokable]
    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStart(GameStartEvent _)
    {
        AvailableUsageTimes = InitialAvailableUsableTimes.GetInt();
        PlayerUtils.GetAllPlayers().ForEach(target => _prefixes.Add(target.PlayerId, target.Data.PlayerName));
    }

    private void ShowCurrentCamp(PlayerControl target)
    {
        if (_prefixes.TryGetValue(target.PlayerId, out var prefix))
        {
            PlayerUtils.SetNamePrivately(target, PlayerControl.LocalPlayer, prefix + $"({target.GetMainRole().CampType.GetName()})");
        }
    }
    
    public override void ClearRoleGameData()
    {
        _checkedPlayers.Clear();
        _prefixes.Clear();
    }
}