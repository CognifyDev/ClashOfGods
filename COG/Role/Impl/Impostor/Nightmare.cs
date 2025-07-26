using COG.Rpc;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using COG.Utils.Coding;
using InnerNet;
using Reactor.Utilities;
using System.Collections;

namespace COG.Role.Impl.Impostor;

[NotUsed]
[NotTested]
[WorkInProgress]
[Todo("")]
public class Nightmare : CustomRole
{
    private CustomOption _storeCooldown;
    private CustomButton _storeButton;
    private RpcHandler<PlayerControl> _storeHandler;
    private RpcHandler<PlayerControl, PlayerControl, float> _cooldownCheckHandler;

    // GAMEPLAY VARIABLES
    private int _storedKills = 0;
    private float _teammateCurrentCooldown = float.MinValue;
    private bool _receivedCooldownSent = false;
    private PlayerControl? _target;

    private const int MaxKillsStored = 2;

    public Nightmare() : base()
    {
        _storeHandler = new(KnownRpc.NightmareStore,
            player =>
            {
                if (player.AmOwner) // local player only
                    player.ResetKillCooldown();
            },
            (writer, player) => writer.WriteNetObject(player),
            reader => reader.ReadNetObject<PlayerControl>());
        _cooldownCheckHandler = new(KnownRpc.NightmareCooldownCheck,
            (sender, receiver, cooldown) =>
            {
                if (!receiver.AmOwner) return;

                if (cooldown == float.MinValue)
                {
                    _cooldownCheckHandler!.Send(receiver, sender, PlayerControl.LocalPlayer.killTimer); // Sending current cooldown
                }
                else
                {
                    _teammateCurrentCooldown = cooldown;
                    _receivedCooldownSent = true;
                }
            },
            (writer, player, receiver, cooldown) => writer.WriteNetObject(player).Write(cooldown),
            reader => (reader.ReadNetObject<PlayerControl>(), reader.ReadNetObject<PlayerControl>(), reader.ReadSingle()));

        RegisterRpcHandler(_storeHandler);
        RegisterRpcHandler(_cooldownCheckHandler);

        _storeCooldown = CreateOption(() => GetContextFromLanguage("store-cooldown"), 
            new FloatOptionValueRule(10, 5, 60, 20, NumberSuffixes.Seconds));

        _storeButton = CustomButton.Of("nightmare-store",
            () =>
            {
                Coroutines.Start(Ability());

                IEnumerator Ability()
                {
                    _storeHandler.Send(_target!);
                    _cooldownCheckHandler.Send(PlayerControl.LocalPlayer, _target!, float.MinValue);

                    while (!_receivedCooldownSent)
                        yield return null; // Wait for receiving cooldown

                    if (_teammateCurrentCooldown <= 0)
                    {
                        CurrentKillButtonSetting = new();
                        CurrentKillButtonSetting.CustomCooldown = () => 0f;
                        CurrentKillButtonSetting.AddAfterClick(() =>
                        {
                            if (--_storedKills == 0)
                                ResetCurrentKillButtonSetting();
                        });

                        _storedKills++;
                    }

                    _teammateCurrentCooldown = float.MinValue;
                    _receivedCooldownSent = false;
                }
            },
            () => { },
            () => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target) && _storedKills < MaxKillsStored,
            () => true,
            null!,
            2,
            ActionNameContext.GetString("store-kill"),
            () => _storeCooldown.GetFloat(),
            0
        );

        AddButton(_storeButton);
    }

    public override void ClearRoleGameData()
    {
        _storedKills = 0;
        _teammateCurrentCooldown = float.MinValue;
        _receivedCooldownSent = false;
    }
}
