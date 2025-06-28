using System.Collections;
using System.Linq;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.Player;
using COG.States;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using Reactor.Utilities;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class SoulHunter : CustomRole, IListener
{
    private const string HasRevivedTag = "hasRevived_SoulHunter";

    private Vector2? _position;
    
    private CustomOption ReviveAfter { get; }
    private CustomOption SoulHunterKillCd { get; }
    
    public SoulHunter() : base(Color.green, CampType.Crewmate)
    {
        ReviveAfter = CreateOption(() => LanguageConfig.Instance.SoulHunterReviveAfter,
            new FloatOptionValueRule(1F, 1F, 60F, 5F, NumberSuffixes.Seconds));
        SoulHunterKillCd = CreateOption(() => LanguageConfig.Instance.KillCooldown,
            new FloatOptionValueRule(1F, 1F, 60F, 20F, NumberSuffixes.Seconds));

        CanKill = true;

        KillButtonSetting.UsesLimit = 1;
        KillButtonSetting.CustomCooldown = SoulHunterKillCd.GetFloat;
    }

    [EventHandler(EventHandlerType.Postfix)]
    [OnlyLocalPlayerInvokable]
    [OnlyInRealGame]
    public void OnPlayerMurder(PlayerMurderEvent @event)
    {
        var target = @event.Target;

        if (!IsLocalPlayerRole(target)) return;
        if (target.HasMarkAs(HasRevivedTag)) return;

        _position = target.GetTruePosition();
        Coroutines.Start(CoRevive());
        return;

        IEnumerator CoRevive()
        {
            yield return new WaitForSeconds(ReviveAfter.GetFloat());

            var deadBody = target.GetDeadBody();

            if (deadBody != null)
                deadBody.RpcHideDeadBody();
            else
                yield break;
            
            if (GameStates.IsMeeting)
                yield break;

            if (_position.HasValue)
                target.NetTransform.RpcSnapTo(_position.Value);
            else
                Main.Logger.LogError($"{nameof(_position)} is null while reviving!");
            
            target.RpcSetCustomRole(this);
            target.RpcRevive();
            target.RpcMark(HasRevivedTag);
            KillButtonSetting.RemainingUses++;
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnReportBody(PlayerReportDeadBodyEvent @event)
    {
        var targets = Players.Where(player => player.HasMarkAs(HasRevivedTag));
        targets.ForEach(target => target.RpcSuicide());
    }

    public override string GetNameInConfig()
    {
        return "soul-hunter";
    }

    public override IListener GetListener()
    {
        return this;
    }
}