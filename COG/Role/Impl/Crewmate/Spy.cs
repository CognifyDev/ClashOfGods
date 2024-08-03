using System.Linq;
using COG.Config.Impl;
using COG.Listener;
using COG.Listener.Event.Impl.Game;
using COG.Utils;
using UnityEngine;

namespace COG.Role.Impl.Crewmate;

public class Spy : CustomRole, IListener
{
    public Spy() : base(LanguageConfig.Instance.SpyName, Color.grey, CampType.Crewmate)
    {
        ShortDescription = LanguageConfig.Instance.SpyDescription;
        LongDescription = LanguageConfig.Instance.SpyLongDescText;
    }

    [EventHandler(EventHandlerType.Postfix)]
    public void OnGameStart(GameStartEvent @event)
    {
        foreach (var player in PlayerUtils.GetAllAlivePlayers())
        {
            if (!player.IsRole<Spy>()) continue;
            var name = player.Data.PlayerName;
            player.RpcSetNamePrivately($"<color=#FF0000>{name}</color>",
                PlayerUtils.GetAllAlivePlayers()
                    .Where(playerControl => playerControl.GetMainRole().CampType == CampType.Impostor).ToArray());
        }
    }

    public override IListener GetListener()
    {
        return this;
    }

    public override CustomRole NewInstance()
    {
        return new Spy();
    }
}