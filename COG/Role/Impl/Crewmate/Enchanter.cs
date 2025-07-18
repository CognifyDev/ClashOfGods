using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.Rpc;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using COG.Utils.Coding;
using InnerNet;
using Reactor.Utilities;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

[NotTested]
[NotUsed]
[Todo("""
    1. optimize killbuttonsetting to avoid conflicts
    """)]
public class Enchanter : CustomRole, IListener
{
    public CustomButton ContractButton { get; }
    public CustomOption ImmobilizationDuration { get; }
    public CustomOption CooldownIncreasement { get; }
    public RpcHandler<PlayerControl> KillerPunishmentHandler { get; }

    private PlayerControl? _contractedPlayer;
    private bool _usedThisRound = false;
    private PlayerControl? _lastKiller;

    public Enchanter() : base(ColorUtils.AsColor("#7B2FF2"), CampType.Crewmate)
    {
        var action = LanguageConfig.Instance.GetHandler("action");

        ContractButton = CustomButton.Of(
            "enchanter-contract",
            () =>
            {
                PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _contractedPlayer);
                _usedThisRound = true;
            },
            () =>
            {
                ContractButton?.ResetCooldown();
                _usedThisRound = false;
                _contractedPlayer = null;
            },
            () => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _contractedPlayer) && !_usedThisRound,
            () => true,
            null!,
            2,
            KeyCode.E,
            action.GetString("contract"),
            () => 0f,
            0
        );

        AddButton(ContractButton);

        ImmobilizationDuration = CreateOption(() => GetContextFromLanguage("immobilization-duration"),
            new FloatOptionValueRule(1, 1, 5,
            3, NumberSuffixes.Seconds));
        CooldownIncreasement = CreateOption(() => GetContextFromLanguage("cooldown-increasement"),
            new FloatOptionValueRule(3, 1, 10,
            5, NumberSuffixes.Seconds));

        KillerPunishmentHandler = new(KnownRpc.EnchanterPunishesKiller,
            p => // p must be local player
            {
                Coroutines.Start(CoImmobilizeAndIncreaseCooldown());

                IEnumerator CoImmobilizeAndIncreaseCooldown()
                {
                    p.moveable = false;
                    yield return new WaitForSeconds(ImmobilizationDuration.GetFloat());
                    p.moveable = true;

                    var role = p.GetRoles().FirstOrDefault(r => r.CanKill);
                    if (role == null) yield break;
                    role.CurrentKillButtonSetting/* this wont be synced, so it is just the setting of local player */.CustomCooldown =
                        () => role.CurrentKillButtonSetting.CustomCooldown() + CooldownIncreasement.GetFloat(); 
                }
            },
            (writer, player) => writer.WriteNetObject(player),
            reader => reader.ReadNetObject<PlayerControl>());

        RegisterRpcHandler(KillerPunishmentHandler);
    }

    [EventHandler(EventHandlerType.Postfix)]
    [OnlyLocalPlayerWithThisRoleInvokable]
    public void OnPlayerMurder(PlayerMurderEvent @event)
    {

    }

    public override IListener GetListener() => this;
}
