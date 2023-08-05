using System.Collections.Generic;
using InnerNet;

namespace COG.Utils;

public abstract class RpcUtils
{
    public static RpcWriter StartRpcImmediately(PlayerControl playerControl, byte callId,
        PlayerControl[]? targets = null)
    {
        var writers = new List<MessageWriter>();
        targets ??= PlayerUtils.GetAllPlayers().ToArray();
        foreach (var control in targets)
            writers.Add(AmongUsClient.Instance.StartRpcImmediately(playerControl.NetId, callId, SendOption.Reliable,
                control.GetClientID()));

        return new RpcWriter(writers.ToArray());
    }

    public class RpcWriter
    {
        private readonly MessageWriter[] _writers;

        internal RpcWriter(MessageWriter[] writers)
        {
            _writers = writers;
        }

        public void Write(bool value)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(value);
        }

        public void Write(byte[] bytes)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(bytes);
        }

        public void Write(string value)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(value);
        }

        public void Write(byte value)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(value);
        }

        public void Write(sbyte value)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(value);
        }

        public void Write(float value)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(value);
        }

        public void Write(int value)
        {
            foreach (var messageWriter in _writers) messageWriter.Write(value);
        }

        public void WritePacked(int value)
        {
            foreach (var messageWriter in _writers) messageWriter.WritePacked(value);
        }

        public void WritePacked(uint value)
        {
            foreach (var messageWriter in _writers) messageWriter.WritePacked(value);
        }

        public void WriteBytesAndSize(byte[] bytes)
        {
            foreach (var messageWriter in _writers) messageWriter.WriteBytesAndSize(bytes);
        }

        public void WriteNetObject(InnerNetObject obj)
        {
            foreach (var messageWriter in _writers) messageWriter.WriteNetObject(obj);
        }

        public void Finish()
        {
            foreach (var messageWriter in _writers) AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }
    }
}