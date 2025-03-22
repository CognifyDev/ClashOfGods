using System.Linq;
using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Constant;
using COG.Listener;
using COG.Listener.Event.Impl.Game;
using COG.Listener.Event.Impl.Player;
using COG.UI.CustomButton;
using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;
using COG.Utils;
using KeyCode = UnityEngine.KeyCode;


namespace COG.Role.Impl.Impostor;

public class Reaper : CustomRole, IListener
{
    private CustomOption TimeToReduce { get; }
    
    private CustomButton KillButton { get; }

    private float _cooldown;
    
    public Reaper() : base(Palette.ImpostorRed, CampType.Impostor)
    {
        BaseRoleType = RoleTypes.Impostor;
        CanKill = true;
        CanVent = true;
        CanSabotage = true;
        
        TimeToReduce = CreateOption(() => LanguageConfig.Instance.ReaperTimeToReduce,
            new FloatOptionValueRule(1F, 0.5F, 15F, 1.5F, NumberSuffixes.Seconds));

        KillButton = CustomButton.Of(
            "reaper-kill",
            () =>
            {
                var target = PlayerControl.LocalPlayer.GetClosestPlayer();
                if (target == null) return;
                PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
            },
            () => KillButton?.ResetCooldown(),
            () =>
            {
                var closestPlayer =
                    PlayerControl.LocalPlayer.GetClosestPlayer(true, GameUtils.GetGameOptions().KillDistance);
                if (closestPlayer == null) return false;

                if (closestPlayer.GetMainRole().CampType == CampType.Impostor &&
                    PlayerControl.LocalPlayer.GetMainRole().CampType == CampType.Impostor) return false;

                return true;
            },
            () =>
            {
                var role = PlayerControl.LocalPlayer.GetPlayerData();
                if (role == null)
                {
                    return false;
                }

                return role.MainRole.CanKill || role.SubRoles.Any(subRole => subRole.CanKill);
            },
            ResourceUtils.LoadSprite(ResourcesConstant.GeneralKillButton)!,
            1,
            KeyCode.Q,
            LanguageConfig.Instance.KillAction,
            () => _cooldown,
            0);
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnPlayerMurder(PlayerMurderEvent @event)
    {
        var target = @event.Target;
        if (target.PlayerId == PlayerControl.LocalPlayer.PlayerId)
        {
            return;
        }

        if (_cooldown > 1)
        {
            _cooldown = TimeToReduce.GetFloat();
        }
    }

    public void OnGameStart(GameStartEvent @event)
    {
        ClearRoleGameData();
    }

    public override void ClearRoleGameData()
    {
        _cooldown = GameUtils.GetGameOptions().KillCooldown;
    }

    public override IListener GetListener()
    {
        return this;
    }
}