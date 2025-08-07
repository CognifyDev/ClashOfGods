using System.Linq;
using COG.Role.Impl.Crewmate;
using COG.Utils;

namespace COG.Game.Events.Impl.Handlers;

public class PlayerDeathHandler : TypeEventHandlerBase
{
    public PlayerDeathHandler() : base(GameEventType.Die)
    {
    }

    public override IGameEvent Handle(CustomPlayerData player, params object[] extraArguments)
    {
        if (extraArguments.Length == 1)
        {
            if (extraArguments[0] is CustomDeathReason reason)
                switch (reason)
                {
                    case CustomDeathReason.InteractionAfterRevival
                        : // This only calls Die method, not MurderPlayer, so write here
                    {
                        var revived = (CustomPlayerData)extraArguments[1];
                        return new WitchRevivedInteractionDieGameEvent(player, revived);
                    }
                    default:
                        return null!;
                }

            if (extraArguments[0] is string extraMessage)
            {
                if (extraMessage.StartsWith(Sheriff.MisfireMurderMessage))
                {
                    if (int.TryParse(extraMessage.Replace(Sheriff.MisfireMurderMessage, ""), out var result))
                    {
                        var playerData = GameUtils.PlayerData.First(p => p.PlayerId == result);
                        return new SheriffMisfireGameEvent(player, playerData);
                    }

                    Main.Logger.LogWarning("Invalid message argument: " + extraMessage);
                    return null!;
                }

                Main.Logger.LogWarning($"Unexpected {nameof(extraMessage)}: {extraMessage}");
                return null!;
            }

            Main.Logger.LogWarning($"Unexpected extra argument type: {extraArguments[0].GetType()}");
            return null!;
        }

        return new PlayerDieGameEvent(player);
    }
}