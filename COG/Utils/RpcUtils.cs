using System.Collections.Generic;
using System.Linq;
using COG.Rpc;
using InnerNet;

namespace COG.Utils;

public abstract class RpcUtils
{
    public static RpcWriter StartRpcImmediately(PlayerControl playerControl, RpcCalls callId,
        PlayerControl[]? targets = null)
    {
        return StartRpcImmediately(playerControl, (byte)callId, targets);
    }

    public static RpcWriter StartRpcImmediately(PlayerControl playerControl, KnownRpc callId,
        PlayerControl[]? targets = null)
    {
        return StartRpcImmediately(playerControl, (byte)callId, targets);
    }

    public static RpcWriter StartRpcImmediately(PlayerControl playerControl, byte callId,
        PlayerControl[]? targets = null)
    {
        var writers = new List<MessageWriter>();
        targets ??= PlayerUtils.GetAllPlayers().Where(p => p != playerControl).ToArray();
        foreach (var control in targets)
        {
            writers.Add(AmongUsClient.Instance.StartRpcImmediately(playerControl.NetId, callId, SendOption.Reliable,
                control.GetClientID()));
            Main.Logger.LogInfo($"Rpc {callId} sent to {control.name}({control.PlayerId})");
        }

        return new RpcWriter(writers.ToArray());
    }

    public class RpcWriter
    {
        private readonly MessageWriter[] _writers;

        internal RpcWriter(MessageWriter[] writers)
        {
            _writers = writers;
        }

        public RpcWriter Write(bool value)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(value);
            return this;
        }

        public RpcWriter Write(byte[] bytes)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(bytes);
            return this;
        }

        public RpcWriter Write(string value)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(value);
            return this;
        }

        public RpcWriter Write(byte value)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(value);
            return this;
        }

        public RpcWriter Write(sbyte value)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(value);
            return this;
        }

        public RpcWriter Write(float value)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(value);
            return this;
        }

        public RpcWriter Write(int value)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(value);
            return this;
        }

        public RpcWriter WritePacked(int value)
        {
            foreach (var messageWriter in _writers) messageWriter.WritePacked(value);
            return this;
        }

        public RpcWriter WritePacked(uint value)
        {
            foreach (var messageWriter in _writers) messageWriter.WritePacked(value);
            return this;
        }

        public RpcWriter WriteBytesAndSize(byte[] bytes)
        {
            foreach (var messageWriter in _writers) messageWriter.WriteBytesAndSize(bytes);
            return this;
        }

        public RpcWriter WriteNetObject(InnerNetObject obj)
        {
            foreach (var messageWriter in _writers) messageWriter.WriteNetObject(obj);
            return this;
        }

        public void Finish()
        {
            foreach (var messageWriter in _writers) AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
    }
}