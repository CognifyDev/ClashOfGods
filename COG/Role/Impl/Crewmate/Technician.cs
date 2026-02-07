using System.Linq;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Attribute;
using COG.Listener.Event.Impl.VentImpl;
using COG.Rpc;
using COG.UI.Hud.CustomButton;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Technician : CustomRole, IListener
{
    public Technician() : base(Palette.Orange, CampType.Crewmate)
    {
        CanVent = true;
        CanKill = false;
        CanSabotage = false;

        var repairRpcHandler = new RpcHandler(KnownRpc.ClearSabotages, RepairSabotages);
        RegisterRpcHandler(repairRpcHandler);

        RepairButton = CustomButton.Builder("technician-repair",
                ResourceConstant.RepairButton, LanguageConfig.Instance.RepairAction)
            .OnClick(repairRpcHandler.PerformAndSend)
            .OnMeetingEnds(() => RepairButton?.ResetCooldown())
            .CouldUse(() => PlayerControl.LocalPlayer.myTasks.ToArray().Any(PlayerTask.TaskIsEmergency))
            .Cooldown(() => 0F)
            .UsesLimit(2)
            .Build();

        AddButton(RepairButton);
    }

    private CustomButton RepairButton { get; }

    [EventHandler(EventHandlerType.Postfix)]
    [OnlyLocalPlayerWithThisRoleInvokable]
    public void OnVentCheck(VentCheckEvent @event)
    {
        var data = @event.PlayerInfo;
        var player = data.Object;
        var vent = @event.Vent;

        if (PlayerControl.LocalPlayer.AllTasksCompleted())
        {
            HudManager.Instance.ImpostorVentButton.Show();

            var dist = float.MaxValue;
            var couldUse = (!player.MustCleanVent(vent.Id) || (player.inVent && Vent.currentVent == vent))
                           && !data.IsDead && (player.CanMove || player.inVent);
            if (ShipStatus.Instance.Systems.TryGetValueSafeIl2Cpp(SystemTypes.Ventilation, out var systemType))
            {
                var ventilationSystem = systemType.Cast<VentilationSystem>();
                if (ventilationSystem != null && ventilationSystem.IsVentCurrentlyBeingCleaned(vent.Id))
                    couldUse = false;
            }

            var canUse = couldUse;
            if (canUse)
            {
                var center = player.Collider.bounds.center;
                var position = vent.transform.position;
                dist = Vector2.Distance(center, position);
                canUse &= dist <= vent.UsableDistance
                          && !PhysicsHelpers.AnythingBetween(player.Collider, center, position, Constants.ShipOnlyMask,
                              false);
            }

            @event.SetCanUse(canUse);
            @event.SetCouldUse(couldUse);
            @event.SetResult(dist);
        }
        else
        {
            @event.SetCanUse(false);
            @event.SetCouldUse(false);
            @event.SetResult(float.MaxValue);
        }
    }

    private static void RepairSabotages()
    {
        var ship = ShipStatus.Instance;
        var mapId = (MapNames)(AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay
            ? AmongUsClient.Instance.TutorialMapId
            : GameUtils.GetGameOptions().MapId);

        ship.RepairCriticalSabotages();
        if (ship.Systems.TryGetValueSafeIl2Cpp(SystemTypes.Electrical, out var system))
        {
            var elecSystem = system.TryCast<SwitchSystem>();
            if (elecSystem != null)
                elecSystem.ActualSwitches = elecSystem.ExpectedSwitches;
        }

        ship.UpdateSystem(SystemTypes.Comms, PlayerControl.LocalPlayer, 16 | 0);
        ship.UpdateSystem(SystemTypes.Comms, PlayerControl.LocalPlayer, 16 | 1);

        if (mapId != MapNames.MiraHQ)
            ship.AllDoors.ForEach(d => d.SetDoorway(true));

        if (mapId == MapNames.Fungle)
        {
            var mixup = ship.Cast<FungleShipStatus>().specialSabotage;
            if (mixup.IsActive)
                mixup.currentSecondsUntilHeal = 0.1f;
        }
    }

    public override IListener GetListener()
    {
        return this;
    }
}