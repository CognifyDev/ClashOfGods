//using System.Collections.Generic;
//using System.Linq;
//using COG.Config.Impl;
//using COG.Constant;
//using COG.Listener;
//using COG.Listener.Attribute;
//using COG.Listener.Event.Impl.Game;
//using COG.Listener.Event.Impl.Modded.Player;
//using COG.Listener.Event.Impl.Player;
//using COG.UI.CustomOption;
//using COG.UI.CustomOption.ValueRules.Impl;
//using COG.UI.Hud.CustomButton;
//using COG.Utils;

//namespace COG.Role.Impl.Crewmate
//{
//    public class Declarer : CustomRole
//    {
//        private CustomButton DeclarButton { get; }
//        private CustomOption Cooldown { get; }

//        private PlayerControl? _target;

//        public Declarer() : base(ColorUtils.AsColor("#515100"), CampType.Crewmate)
//        {
//            Cooldown = CreateOption(() =>
//                    GetContextFromLanguage("check-cooldown"),
//                new FloatOptionValueRule(5, 1, 60, 25, NumberSuffixes.Seconds));

//            var action = new LanguageConfig.TextHandler("action");

//            DeclarButton = CustomButton.Of(
//                "seer-check",
//                () =>
//                {
//                    if (_target == null) return;

//                    ShowCurrentCamp(_current);
//                },
//                () => { },
//                () =>
//                {
//                    PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out var target);
//                    _current = target;
//                    return AvailableUsageTimes > 0 && !HasChecked(_current) && _current != null;
//                },
//                () => true,
//                ResourceUtils.LoadSprite(ResourceConstant.CheckButton),
//                2,
//                action.GetString("check"),
//                () => Cooldown.GetFloat(),
//                -1
//            );

//            AddButton(CheckButton);
//        }

//        [OnlyLocalPlayerWithThisRoleInvokable]
//        [EventHandler(EventHandlerType.Postfix)]
//        public void OnPlayerMurder(PlayerMurderEvent @event)
//        {
//            if (MurderResultFlags.Succeeded != @event.MurderResult!.Value)
//                return;
//            if (PlayerControl.LocalPlayer.IsSamePlayer(@event.Target) && _target.IsSamePlayer(@event.Target))
//            {

//            }
//        }

//        [OnlyLocalPlayerWithThisRoleInvokable]
//        [EventHandler(EventHandlerType.Postfix)]
//        public void OnPlayerRoleChange(PlayerCustomRoleChangeEvent @event)
//        {
//            if (@event.OriginRole.Equals(@event.TargetRole))
//                return;
//            if (!HasChecked(@event.Player))
//                return;

//            ShowCurrentCamp(@event.Player);
//        }

//        [OnlyLocalPlayerWithThisRoleInvokable]
//        [EventHandler(EventHandlerType.Postfix)]
//        public void OnGameStart(GameStartEvent _)
//        {
//            AvailableUsageTimes = (int)InitialAvailableUsableTimes.GetFloat();
//            PlayerUtils.GetAllPlayers().ForEach(target => _prefixes.Add(target.PlayerId, target.Data.PlayerName));
//        }

//        private void ShowTargetRole(PlayerControl target)
//        {
//            if (_prefixes.TryGetValue(target.PlayerId, out var prefix))
//            {
//                target.Data.PlayerName = prefix + $"({target.GetMainRole().CampType.GetName()})";
//            }
//        }
//        private void SetTraget(PlayerControl target)
//        {
//            _target = target;
//        }
//        public override void ClearRoleGameData()
//        {
//            _target = null;
//        }
//    }
//}
