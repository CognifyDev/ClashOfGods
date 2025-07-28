using COG.UI.CustomButton;
using COG.Utils;

namespace COG.Game.Events.Impl;

public class UseAbilityEvent : GameEventBase
{
    public string AbilityButtonIdentifier { get; }

    public UseAbilityEvent(CustomPlayerData player, CustomButton button) : base(EventType.UseAblity, player)
    {
        AbilityButtonIdentifier = button.Identifier;
    }
}
