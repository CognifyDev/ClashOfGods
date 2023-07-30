using COG.Listener;

namespace COG.Patch;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
class RPCHandlerPatch
{
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId,
        [HarmonyArgument(1)] MessageReader reader)
    {
        
        foreach (var listener in ListenerManager.GetManager().GetListeners())
        {
            listener.OnRPCReceived(callId, reader);
        }
        
        var rpcType = (RpcCalls)callId;
        var subReader = MessageReader.Get(reader);
        switch (rpcType)
        {
            case RpcCalls.SendChat:
                var text = subReader.ReadString();
                bool returnAble = false;
                foreach (var listener in ListenerManager.GetManager().GetListeners())
                {
                    if (!listener.OnPlayerChat(__instance, text) && !returnAble)
                    {
                        returnAble = true;
                    }
                }

                if (returnAble) return false;
                break;
        }

        return true;
    }
}