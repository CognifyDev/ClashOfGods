using COG.States;
using COG.Utils;

namespace COG.Listener.Impl;

public class GameListener : IListener
{
    public void OnCoBegin()
    {
        GameStates.InGame = true;
        
        // 施工中
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
        // 施工中
    }
}