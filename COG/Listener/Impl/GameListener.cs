using COG.Role;
using COG.States;
using COG.Utils;

namespace COG.Listener.Impl;

public class GameListener : IListener
{
    public void OnCoBegin()
    {
        GameStates.InGame = true;
        var players = PlayerUtils.GetAllPlayers();
        var impostorNum = GameUtils.GetGameOptions().NumImpostors;
        for (var i = 0; i < players.Count; i++)
        {
            var roles = Role.RoleManager.GetManager().GetRoles();
            var role = roles.Count - 1 > i ? roles[i] : Role.RoleManager.GetManager().GetDefaultRole(impostorNum <= 0 ? Camp.Crewmate : Camp.Impostor);
            if (role.Camp == Camp.Impostor) impostorNum --;
            GameUtils.Data.Add(players[i], role);
        }
    }

    public void OnGameEnd(AmongUsClient client, EndGameResult endGameResult)
    {
        GameStates.InGame = false;
    }

    public void OnGameStart(GameStartManager manager)
    {
        // 改变按钮颜色
        manager.MakePublicButton.color = Palette.DisabledClear;
        manager.privatePublicText.color = Palette.DisabledClear;
    }

    public bool OnMakePublic(GameStartManager manager)
    {
        GameUtils.SendGameMessage("禁止设置为公开");
        // 禁止设置为公开
        return false;
    }

    public void OnSetUpRoleText(IntroCutscene intro)
    {
        // 游戏开始的时候显示角色信息
        
    }
}