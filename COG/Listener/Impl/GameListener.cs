using COG.Config.Impl;
using COG.States;
using COG.Utils;

namespace COG.Listener.Impl;

public class GameListener : IListener
{
    public void OnCoBegin()
    {
        GameStates.InGame = true;
    }

    public void OnSelectRoles()
    {
        
    }

    public void OnGameEnd(AmongUsClient client, EndGameResult endGameResult)
    {
        
    }

    public void OnGameStart(GameStartManager manager)
    {
        // 改变按钮颜色
        manager.MakePublicButton.color = Palette.DisabledClear;
        manager.privatePublicText.color = Palette.DisabledClear;
    }

    public bool OnMakePublic(GameStartManager manager)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        GameUtils.SendGameMessage(LanguageConfig.Instance.MakePublicMessage);
        // 禁止设置为公开
        return false;
    }

    public void OnSetUpRoleText(IntroCutscene intro)
    {
        
    }

    public void OnSetUpTeamText(IntroCutscene intro)
    {
        
    }

    public void OnGameEndSetEverythingUp(EndGameManager manager)
    {
        
    }
}