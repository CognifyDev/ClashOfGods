using COG.Config.Impl;
using COG.Listener;
using COG.Modules;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl;

public class Jester : Role, IListener
{
    private PlayerControl? _player;
    
    public Jester() : base(LanguageConfig.Instance.JesterName, Color.magenta, true, CampType.Neutral)
    {
        Description = LanguageConfig.Instance.JesterDescription;
        var parentOption = RoleOptions[0];
        RoleOptions.Add(CustomOption.Create(
            parentOption.ID + 1, ToCustomOption(this), LanguageConfig.Instance.AllowStartMeeting, true, parentOption)
        );
    }

    public bool OnPlayerReportDeadBody(PlayerControl playerControl, GameData.PlayerInfo? target)
    {
        var allowReportDeadBodyOption = RoleOptions[1];
        var result = allowReportDeadBodyOption.GetBool();
        if (!result && playerControl.FriendCode.Equals(_player!.FriendCode) && target == null)
        {
            return false;
        }

        return true;
    }

    private bool CheckNull()
    {
        return _player == null;
    }

    public void OnPlayerExile(ExileController controller)
    {
        if (CheckNull()) return;
        if (!controller.exiled.IsSamePlayer(_player!.Data)) return;
        
        GameManager.Instance.RpcEndGame(GameOverReason.HumansByVote, true);
    }

    public void OnAirshipPlayerExile(AirshipExileController controller)
    {
        OnPlayerExile(controller);
    }

    public override IListener GetListener(PlayerControl player)
    {
        _player = player;
        return this;
    }
}