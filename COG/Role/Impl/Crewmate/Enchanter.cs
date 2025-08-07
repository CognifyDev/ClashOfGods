using System.Collections;
using System.Linq;
using COG.Listener;
using COG.Listener.Attribute;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.Utils;
using InnerNet;
using Reactor.Utilities;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Enchanter : CustomRole, IListener
{
    private PlayerControl? _contractedPlayer;
    private PlayerControl? _lastKiller;
    private PlayerControl? _target;
    private bool _usedThisRound;

    public Enchanter() : base(ColorUtils.AsColor("#7030a0"), CampType.Crewmate)
    {
        ContractButton = CustomButton.Of(
            "enchanter-contract",
            () =>
            {
                _contractedPlayer = _target;
                _usedThisRound = true;
            },
            () =>
            {
                _usedThisRound = false;
                _contractedPlayer = null;
            },
            () => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target) && !_usedThisRound,
            () => true,
            null!,
            2,
            ActionNameContext.GetString("contract"),
            () => 0f,
            0
        );

        AddButton(ContractButton);

        ImmobilizationDuration = CreateOption(() => GetContextFromLanguage("immobilization-duration"),
            new FloatOptionValueRule(1, 1, 5,
                3, NumberSuffixes.Seconds));
        CooldownIncreament = CreateOption(() => GetContextFromLanguage("cooldown-increament"),
            new FloatOptionValueRule(3, 1, 10,
                5, NumberSuffixes.Seconds));

        KillerPunishmentHandler = new RpcHandler<PlayerControl>(KnownRpc.EnchanterPunishesKiller,
            p => // p must be local player
            {
                if (!p.AmOwner) return;

                Coroutines.Start(CoImmobilizeAndIncreaseCooldown());

                IEnumerator CoImmobilizeAndIncreaseCooldown()
                {
                    Main.Logger.LogDebug("Setting unmoveable");

                    var duration = ImmobilizationDuration.GetFloat();
                    while (duration > 0f)
                    {
                        p.moveable = false; // directly setting moveable is useless
                        duration -= Time.deltaTime;
                        yield return null;
                    }

                    p.moveable = true;

                    Main.Logger.LogDebug("Increasing cooldown");
                    var role = p.GetRoles().FirstOrDefault(r => r.CanKill);
                    if (role == null) yield break;

                    var originCooldown = role.CurrentKillButtonSetting.CustomCooldown();
                    role.CurrentKillButtonSetting /* this wont be synced, so it is just the setting of local player */
                            .CustomCooldown =
                        () => originCooldown + CooldownIncreament.GetFloat();
                    PlayerControl.LocalPlayer.ResetKillCooldown();
                }
            },
            (writer, player) => writer.WriteNetObject(player),
            reader => reader.ReadNetObject<PlayerControl>());

        RegisterRpcHandler(KillerPunishmentHandler);
    }

    public CustomButton ContractButton { get; }
    public CustomOption ImmobilizationDuration { get; }
    public CustomOption CooldownIncreament { get; }
    public RpcHandler<PlayerControl> KillerPunishmentHandler { get; }

    [EventHandler(EventHandlerType.Postfix)]
    [OnlyLocalPlayerWithThisRoleInvokable]
    public void OnPlayerMurder(PlayerMurderEvent @event)
    {
        if (!_contractedPlayer) return;
        if (@event.Target.IsSamePlayer(_contractedPlayer))
        {
            _lastKiller = @event.Player;
            KillerPunishmentHandler.Send(_lastKiller);
            _lastKiller = null;
            _contractedPlayer = null;
        }
    }

    public override void ClearRoleGameData()
    {
        _contractedPlayer = null;
        _target = null;
        _usedThisRound = false;
        _lastKiller = null;
    }

    public override IListener GetListener()
    {
        return this;
    }
}