using System.Collections;
using System.Linq;
using COG.Constant;
using COG.Listener;
using COG.Listener.Attribute;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.Rpc.Role;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.Utils;
using InnerNet;
using Reactor.Utilities;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Enchanter : COG.Role.Camp.CrewmateRole
{
    private PlayerControl? _contractedPlayer;
    private PlayerControl? _lastKiller;
    private PlayerControl? _target;
    private bool _usedThisRound;

    public Enchanter() : base(ColorUtils.AsColor("#7030a0"))
    {
        ContractButton = CustomButton.Builder("enchanter-contract",
                ResourceConstant.ContractButton, ActionNameContext.GetString("contract"))
            .OnClick(() =>
            {
                _contractedPlayer = _target;
                _usedThisRound = true;
            })
            .OnMeetingEnds(() =>
            {
                _usedThisRound = false;
                _contractedPlayer = null;
            })
            .CouldUse(() => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target) && !_usedThisRound)
            .Cooldown(() => 0F)
            .Build();

        AddButton(ContractButton);

        ImmobilizationDuration = CreateOption(() => GetContextFromLanguage("immobilization-duration"),
            new FloatOptionValueRule(1, 1, 5,
                3, NumberSuffixes.Seconds));
        CooldownIncreament = CreateOption(() => GetContextFromLanguage("cooldown-increament"),
            new FloatOptionValueRule(3, 1, 10,
                5, NumberSuffixes.Seconds));

        KillerPunishmentRpc = CreateRoleRpc<PlayerControl>(KnownRpc.EnchanterPunishesKiller,
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

                    var baseCooldown = role.CurrentKillButtonSetting.CustomCooldown();
                    role.CurrentKillButtonSetting /* this wont be synced, so it is just the setting of local player */
                            .CustomCooldown =
                        () => baseCooldown + CooldownIncreament.GetFloat();
                    PlayerControl.LocalPlayer.ResetKillCooldown();
                }
            });

        On<PlayerMurderEvent>(OnPlayerMurder);
    }

    public CustomButton ContractButton { get; }
    public CustomOption ImmobilizationDuration { get; }
    public CustomOption CooldownIncreament { get; }
    public RoleRpc<PlayerControl> KillerPunishmentRpc { get; }

    [LocalOnly]
    private void OnPlayerMurder(PlayerMurderEvent @event)
    {
        if (!_contractedPlayer) return;
        if (@event.Target.IsSamePlayer(_contractedPlayer))
        {
            _lastKiller = @event.Player;
            KillerPunishmentRpc.Send(_lastKiller);
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
}