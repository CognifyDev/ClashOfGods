using COG.Modules;
using InnerNet;

namespace COG.Listener.Impl;

public class RPCListener : IListener
{
    public void OnRPCReceived(byte callId, MessageReader reader)
    {
        var rpc = (CustomRPC)callId;
        switch (rpc)
        {
            case CustomRPC.ShareRoles:
                // var data = reader.ReadNetObject<>()
                break;
        }
    }
}