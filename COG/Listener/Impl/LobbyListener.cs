using COG.Config.Impl;
using COG.Listener.Event.Impl.GSManager;
using COG.Utils;
using UnityEngine;

namespace COG.Listener.Impl;

public class LobbyListener : IListener
{
    [EventHandler(EventHandlerType.Postfix)]
    public void OnJoiningLobby(GameStartManagerStartEvent @event)
    {
        var manager = @event.GameStartManager;
        var privateButton = manager.HostPrivacyButtons.transform.FindChild("PRIVATE BUTTON");
        var inactive = privateButton.FindChild("Inactive").GetComponent<SpriteRenderer>();
        var highlight = privateButton.FindChild("Highlight").GetComponent<SpriteRenderer>();
        inactive.color = highlight.color = Palette.DisabledGrey;
    }

    [EventHandler(EventHandlerType.Prefix)]
    public bool OnMakingGamePublic(GameStartManagerMakePublicEvent _)
    {
        if (!AmongUsClient.Instance.AmHost) return false;
        GameUtils.SendGameMessage(LanguageConfig.Instance.MakePublicMessage);
        // 禁止设置为公开
        return false;
    }
}