using COG.UI.CustomButton;
using COG.Utils;

namespace COG.Game.Events.Impl;

public class UseAbilityGameEvent : GameEventBase
{
    public string AbilityButtonIdentifier { get; }

    public UseAbilityGameEvent(CustomPlayerData player, CustomButton button) : base(GameEventType.UseAblity, player)
    {
        AbilityButtonIdentifier = button.Identifier;
    }
}
