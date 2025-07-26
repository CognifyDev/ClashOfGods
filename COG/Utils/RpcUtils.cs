using COG.Rpc;
using InnerNet;
using UnityEngine;

namespace COG.Utils;

public static class RpcUtils
{
    public static RpcWriter StartRpcImmediately(this PlayerControl playerControl, RpcCalls callId,
        PlayerControl[]? targets = null)
    {
        return RpcWriter.Start(playerControl, (byte)callId, targets);
    }

    public static RpcWriter StartRpcImmediately(this PlayerControl playerControl, KnownRpc callId,
        PlayerControl[]? targets = null)
    {
        return RpcWriter.Start(playerControl, (byte)callId, targets);
    }

    public static RpcWriter StartRpcImmediately(this PlayerControl playerControl, byte callId,
        PlayerControl[]? targets = null)
    {
        return RpcWriter.Start(playerControl, callId, targets);
    }

    public static Vector2 ReadVector<T>(this MessageReader reader)
    {
        return NetHelpers.ReadVector2(reader);
    }

    public static PlayerControl ReadPlayerControl(this MessageReader reader)
    {
        return reader.ReadNetObject<PlayerControl>();
    }
}