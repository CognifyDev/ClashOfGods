using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomButton;
using COG.Utils;
using COG.Utils.Coding;
using System.Linq;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

[WorkInProgress]
public class Technician : CustomRole
{
    public Technician() : base(Palette.Orange, CampType.Crewmate)
    {
        RepairButton = CustomButton.Create(() =>
        {
            var ship = ShipStatus.Instance;
            var mapId = (MapNames)(AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay ?
            AmongUsClient.Instance.TutorialMapId :
            GameUtils.GetGameOptions().MapId);

            ship.RepairCriticalSabotages();
            if (ship.Systems.TryGetValueSafeIl2Cpp(SystemTypes.Electrical, out var system))
            {
                var elecSystem = system.TryCast<SwitchSystem>();
                if (elecSystem != null) 
                    elecSystem.ActualSwitches = elecSystem.ExpectedSwitches;
            }

            ship.UpdateSystem(SystemTypes.Comms, PlayerControl.LocalPlayer, 16 | 0);
            ship.UpdateSystem(SystemTypes.Comms, PlayerControl.LocalPlayer, 16 | 1);

            if (mapId != MapNames.Mira)
                ship.AllDoors.ForEach(d => d.SetDoorway(true));

            if (mapId == MapNames.Fungle)
            {
                var mixup = ship.Cast<FungleShipStatus>().specialSabotage;
                if (mixup.IsActive)
                    mixup.currentSecondsUntilHeal = 0.1f;
            }
        },
        () => RepairButton.ResetCooldown(),
        () => true,
        () => true,
        ResourceUtils.LoadSprite("COG.Resources.InDLL.Images.Buttons.Repair.png")!,
        2,
        KeyCode.F,
        "REPAIR",
        () => 0f,
        2
        );

        AddButton(RepairButton);
    }

    private CustomButton RepairButton { get; }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerFixedUpdate(PlayerFixedUpdateEvent _)
    {
        var ventButton = HudManager.Instance.ImpostorVentButton;
        if (PlayerControl.LocalPlayer.AllTasksCompleted())
        {
            
        }
    }

    public override CustomRole NewInstance()
    {
        return new Technician();
    }
}