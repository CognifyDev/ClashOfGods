using System.Linq;
using COG.Game.Events;
using COG.Rpc;
using COG.UI.Hud.CustomButton;
using COG.Utils;

namespace COG.Listener.Event.Impl.Game.Record;

public class UseAbilityGameEvent(CustomPlayerData player, CustomButton button)
    : NetworkedGameEventBase<UseAbilityGameEvent, UseAbilityEventSender>(GameEventType.UseAbility, player)
{
    public CustomButton UsedButton { get; } = button;
}

public class UseAbilityEventSender : NetworkedGameEventSender<UseAbilityEventSender, UseAbilityGameEvent>
{
    public override void Serialize(RpcWriter writer, UseAbilityGameEvent correspondingEvent)
    {
        writer.WriteBytesAndSize(SerializablePlayerData.Of(correspondingEvent.Player!).SerializeToData());
        writer.Write(correspondingEvent.UsedButton.Identifier);
    }

    public override UseAbilityGameEvent Deserialize(MessageReader reader)
    {
        var rawPlayerData = reader.ReadBytesAndSize();
        var playerData = rawPlayerData.ToArray().DeserializeToData<SerializablePlayerData>().ToPlayerData();

        var buttonString = reader.ReadString();
        var button = CustomButtonManager.GetManager().GetButtons().First(b => b.Identifier == buttonString);

        return new UseAbilityGameEvent(playerData, button);
    }
}