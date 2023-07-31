using AmongUs.GameOptions;
using COG.Config.Impl;
using COG.Listener;
using COG.Utils;

namespace COG.Role.Impl.Crewmate;

public class Sheriff : Role, IListener
{
    private PlayerControl _player = null!;
    
    public Sheriff() : base(LanguageConfig.Instance.SheriffName, UnityEngine.Color.yellow, CampType.Crewmate, true)
    {
        CanVent = false;
        CanKill = true;
        CanSabotage = false;
        BaseRoleType = RoleTypes.Impostor;
        Description = LanguageConfig.Instance.SheriffDescription;
    }

    public void OnCoBegin()
    {
        _player.RpcSetName($"<color=#FFFFF>{_player.name}</color>");
    }

    public bool OnCheckMurder(PlayerControl killer, PlayerControl target)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        var role = killer.GetRoleInstance();
        return role == null || role.Name.Equals(Name);
    }

    public bool OnPlayerMurder(PlayerControl killer, PlayerControl target)
    {
        if (killer == null || target == null) return true;
        if (killer.GetRoleInstance()!.Name.Equals(Name))
        {
            if (target.GetRoleInstance()!.CampType == CampType.Crewmate)
            {
                killer.MurderPlayer(killer);
            }
        }

        return true;
    }

    public override IListener GetListener(PlayerControl player)
    {
        _player = player;
        return this;
    }
}