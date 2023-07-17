using System.Linq;

namespace COG.Modules
{
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
                    CustomOption option = CustomOption.options.First(option => option.id == (int)optionId);
                    option.updateSelection((int)selection);
                }
            }
            catch (System.Exception e)
            {
                Main.Logger.LogError("Error while deserializing options: " + e.Message);
            }
        }
    }
}
