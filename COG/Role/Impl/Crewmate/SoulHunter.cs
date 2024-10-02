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
    
    private readonly CustomButton _killButton;

    private Vector3? _position;
    
    private CustomOption ReviveAfter { get; }
    private CustomOption SoulHunterKillCd { get; }
    
    public SoulHunter() : base(Color.green, CampType.Crewmate)
    {
        ReviveAfter = CreateOption(() => LanguageConfig.Instance.SoulHunterReviveAfter,
            new IntOptionValueRule(1, 1, 60, 5));
        SoulHunterKillCd = CreateOption(() => LanguageConfig.Instance.KillCooldown,
            new IntOptionValueRule(1, 1, 40, 20));
        
        _killButton = CustomButton.Create(
            () =>
            {
                var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                if (target == null) return;
                PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
                _killButton!.UsesRemaining --;
            },
            () => _killButton!.ResetCooldown(),
            () => 
                PlayerControl.LocalPlayer.GetClosestPlayer(true, GameUtils.GetGameOptions().KillDistance)
            && _killButton!.UsesRemaining > 0,
            () => true,
            ResourceUtils.LoadSprite(ResourcesConstant.GeneralKillButton)!,
            2,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            () => SoulHunterKillCd.GetInt(),
            -1);

        _killButton.UsesRemaining = 1;
        
        AddButton(_killButton);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerMurder(PlayerMurderEvent @event)
    {
        if (!GameStates.InGame) return;
        var target = @event.Target;
        if (!IsLocalPlayerRole(target)) return;
        if (target.HasMarkAs(HasRevivedTag)) return;
        _position = target.transform.position;
        Coroutines.Start(Revive());
        return;

        IEnumerator Revive()
        {
            yield return new WaitForSeconds(ReviveAfter.GetInt());
            var deadBody = target.GetDeadBody();
            if (deadBody != null)
            {
                deadBody.RpcCleanDeadBody();
            }
            else
            {
                yield break;
            }

            if (GameStates.IsMeeting)
            {
                yield break;
            }
            
            if (_position != null)
            {
                target.transform.position = (Vector3) _position;
            }
            target.RpcSetRole(BaseRoleType);
            target.RpcSetCustomRole(this);
            target.RpcRevive();
            target.RpcMark(HasRevivedTag);
            _killButton.UsesRemaining ++;
        }
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnReportBody(PlayerReportDeadBodyEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost) return;
        var targets = PlayerUtils.GetAllAlivePlayers().Where(player => player.HasMarkAs(HasRevivedTag));
        targets.ForEach(target =>
        {
            target.RpcMurderPlayer(target, true);
        });
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