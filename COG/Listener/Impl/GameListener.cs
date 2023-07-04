using COG.States;
using COG.Utils;

namespace COG.Listener.Impl;

public class GameListener : IListener
{
    public void OnCoBegin()
    {
        GameStates.InGame = true;
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
        GameUtils.SendGameMessage(Language.GetLang("MakePublic", Language.GetCorrectSupportedLanguage()));
        // 禁止设置为公开
        return false;
    }
}