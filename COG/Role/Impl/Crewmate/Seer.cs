using System;
using System.Linq;
using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Attribute;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.UI.Vanilla.KillButton;
using COG.Utils;
using InnerNet;
using UnityEngine;

namespace COG.Role.Impl.Crewmate
{
    public class Seer : CustomRole
    {
        private readonly CustomButton _checkButton;
        public PlayerControl? _target;
        public int Num { get; set; }
        public CustomOption Cooldown { get; }
        public Seer() : base(new Color(30,144,255), CampType.Crewmate)
        {
            Cooldown = CreateOption(() => 
            GetContextFromLanguage("check-cooldown"),
            new FloatOptionValueRule(5, 1, 60, 25, NumberSuffixes.Seconds));

            var action = new LanguageConfig.TextHandler("action");

            _checkButton = CustomButton.Of(
                "seer-check",
                () =>
                {
                    _target!.Data.PlayerName += $"({Enum.GetName(_target!.GetMainRole().CampType)})";
                    Num--;
                }, 
                () => { },
                () => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target) && Num >= 0,
                () => true,
                null!,
                2,
                action.GetString("check"),
                () => Cooldown.GetFloat(),
                Num
            );
 
            AddButton(_checkButton);
        }
        [EventHandler(EventHandlerType.Postfix)]
        public void OnPlayerMurder(PlayerMurderEvent @event)
        {
            if (!_target) return;
            if (@event.Target.IsSamePlayer(_target)) Num++;
        }
    }
}
