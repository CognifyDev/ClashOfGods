using System.Linq;
using COG.Listener;

namespace COG.Patch;

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
internal class RPCHandlerPatch
{
    [HarmonyPostfix]
    public static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
    {
        Main.Logger.LogInfo($"Rpc {callId} received, rpc length => {reader.Length}");
        foreach (var listener in ListenerManager.GetManager().GetListeners().ToList())
            listener.AfterRPCReceived(callId, reader);
    }

    [HarmonyPrefix]
    public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte callId,
        [HarmonyArgument(1)] MessageReader reader)
    {
        foreach (var listener in ListenerManager.GetManager().GetListeners()) listener.OnRPCReceived(callId, reader);

        var rpcType = (RpcCalls)callId;
        var subReader = MessageReader.Get(reader);
        switch (rpcType)
        {
            case RpcCalls.SendChat:
                var text = subReader.ReadString();
                var returnAble = false;
                foreach (var unused in ListenerManager.GetManager().GetListeners()
                             .Where(listener => !listener.OnPlayerChat(__instance, text) && !returnAble))
                    returnAble = true;

                if (returnAble) return false;
                break;
        }

        return true;
    }
}