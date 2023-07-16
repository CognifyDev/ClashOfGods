using COG.Listener;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl;

public class Jester : Role, IListener
{
    private PlayerControl? _player;
    
    public Jester() : base(3)
    {
        Name = "Jester";
        Description = "You'll win when you get exiled.";
        Color = Color.magenta;
        CampType = CampType.Neutral;
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