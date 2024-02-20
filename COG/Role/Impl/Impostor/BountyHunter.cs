using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace COG.Role.Impl.Impostor
{
    public class BountyHunter : Role, IListener
    {
        public CustomButton BHunterKillButton { get; set; }
        public CustomOption BHunterKillCd { get; set; }
        public BountyHunter() : base("BountyHunter", Palette.ImpostorRed, CampType.Impostor, true)
        {
            BHunterKillButton = CustomButton.Create(
                () =>
                {

                },
                () => BHunterKillButton?.ResetCooldown(),
                couldUse: () =>
                {
                    var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                    if (target == null) return false;
                    var localPlayer = PlayerControl.LocalPlayer;
                    var localLocation = localPlayer.GetTruePosition();
                    var targetLocation = target.GetTruePosition();
                    var distance = Vector2.Distance(localLocation, targetLocation);
                    return GameUtils.GetGameOptions().KillDistance >= distance;
                },
                () => true,
                ResourceUtils.LoadSpriteFromResources("COG.Resources.InDLL.Images.Buttons.GeneralKill.png", 100f)!,
                row: 2,
                KeyCode.Q,
                LanguageConfig.Instance.KillAction,
                (Cooldown)BHunterKillCd!.GetFloat,
                -1
            );
        }


        [EventHandler(EventHandlerType.Postfix)]
        public void AfterPlayerFixedUpdate(PlayerFixedUpdateEvent @event)
        {

        }
    }
}
