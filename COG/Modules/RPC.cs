using COG.Listener;

namespace COG.Modules;

enum CustomRPC
{
    ShareOptions = 200,
}
[HarmonyPatch(typeof(PlayerControl),nameof(PlayerControl.HandleRpc))]
class RPCHandler
{
    public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        CustomRPC rpc = (CustomRPC)callId;
        switch (rpc)
        {
            case CustomRPC.ShareOptions:
                RPCProcedure.HandleShareOptions(reader.ReadByte(), reader);
                break;
        }
            
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            listener.OnRPCReceived(callId, reader);
        }
    }
}
class RPCProcedure
{
    public static void HandleShareOptions(byte numberOfOptions, MessageReader reader)
    {
        try
        {
            for (int i = 0; i < numberOfOptions; i++)
            {
                uint optionId = reader.ReadPackedUInt32();
                uint selection = reader.ReadPackedUInt32();
                // CustomOption? option = CustomOption.Options.First(option => option.ID == (int)optionId);
                // option.UpdateSelection((int)selection);
            }
        }
        catch (System.Exception e)
        {
            Main.Logger.LogError("Error while deserializing options: " + e.Message);
        }
    }
}