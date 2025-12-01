using System.Collections;
using COG.Rpc;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.UI.Hud.CustomButton;
using COG.UI.Vanilla.KillButton;
using COG.Utils;
using COG.Utils.Coding;
using InnerNet;
using Reactor.Utilities;
using UnityEngine;

namespace COG.Role.Impl.Impostor;

[NotTested("rpc")]
public class Nightmare : CustomRole
{
    private const int MaxKillsStored = 2;
    private const float ResponseTimeout = 5f;
    private readonly RpcHandler<PlayerControl, PlayerControl, float> _cooldownCheckHandler;
    private readonly CustomButton _storeButton;
    private readonly CustomOption _storeCooldown;
    private readonly RpcHandler<PlayerControl> _storeHandler;
    private bool _receivedCooldownSent;

    // GAMEPLAY VARIABLES
    private int _storedKills;
    private PlayerControl? _target;
    private float _teammateCurrentCooldown = float.MaxValue;

    public Nightmare()
    {
        _storeHandler = new RpcHandler<PlayerControl>(KnownRpc.NightmareStore,
            player =>
            {
                if (player.AmOwner) // local player only
                    player.ResetKillCooldown();
            },
            (writer, player) => writer.WriteNetObject(player),
            reader => reader.ReadNetObject<PlayerControl>());
        _cooldownCheckHandler = new RpcHandler<PlayerControl, PlayerControl, float>(KnownRpc.NightmareCooldownCheck,
            (sender, receiver, cooldown) =>
            {
                if (!receiver.AmOwner) return;

                if (cooldown == float.MaxValue)
                {
                    _cooldownCheckHandler!.Send(receiver, sender,
                        PlayerControl.LocalPlayer.killTimer); // Sending current cooldown
                }
                else
                {
                    _teammateCurrentCooldown = cooldown;
                    _receivedCooldownSent = true;
                }
            },
            (writer, player, receiver, cooldown) => writer.WriteNetObject(player).Write(cooldown),
            reader => (reader.ReadNetObject<PlayerControl>(), reader.ReadNetObject<PlayerControl>(),
                reader.ReadSingle()));

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
                    _cooldownCheckHandler.Send(PlayerControl.LocalPlayer, _target!, float.MaxValue);

                    Main.Logger.LogInfo("Begun checking teammate cooldown, waiting for response...");

                    var timer = 0f;
                    while (!_receivedCooldownSent || timer <= ResponseTimeout)
                    {
                        yield return null; // Wait for receiving cooldown
                        timer += Time.deltaTime;
                    }

                    if (timer > ResponseTimeout)
                        Main.Logger.LogWarning("Teammate cooldown check timed out");


                    if (_teammateCurrentCooldown <= 0)
                    {
                        CurrentKillButtonSetting = new KillButtonSetting();
                        CurrentKillButtonSetting.CustomCooldown = () => 0f;
                        CurrentKillButtonSetting.AddAfterClick(() =>
                        {
                            if (--_storedKills == 0)
                                ResetCurrentKillButtonSetting();
                            SyncInfoText();
                        });

                        _storedKills++;
                        SyncInfoText();
                    }

                    _teammateCurrentCooldown = float.MaxValue;
                    _receivedCooldownSent = false;

                    void SyncInfoText()
                    {
                        _storeButton!.SetInfoText(GetContextFromLanguage("stored-kills-info")
                            .CustomFormat(("stored", _storedKills), ("max", MaxKillsStored)));
                    }
                }
            },
            () => { },
            () => PlayerControl.LocalPlayer.CheckClosestTargetInKillDistance(out _target) &&
                  _storedKills < MaxKillsStored && _target!.GetMainRole().CampType == CampType.Impostor,
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
        _teammateCurrentCooldown = float.MaxValue;
        _receivedCooldownSent = false;
    }
}