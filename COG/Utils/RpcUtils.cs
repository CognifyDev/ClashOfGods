global using RpcWriter = COG.Utils.RpcUtils.RpcWriter;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using COG.Rpc;
using InnerNet;
using UnityEngine;

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

    public static RpcWriter StartRpcImmediately(KnownRpc callId,
        PlayerControl[]? targets = null)
    {
        return StartRpcImmediately(PlayerControl.LocalPlayer, (byte)callId, targets);
    }

    public static RpcWriter StartRpcImmediately(RpcCalls callId,
        PlayerControl[]? targets = null)
    {
        return StartRpcImmediately(PlayerControl.LocalPlayer, (byte)callId, targets);
    }

    public static RpcWriter StartRpcImmediately(PlayerControl playerControl, byte callId,
        PlayerControl[]? targets = null)
    {
        List<string> parts = new();

        var writers = new List<MessageWriter>();
        targets ??= PlayerUtils.GetAllPlayers().Where(p => p.PlayerId != playerControl.PlayerId).ToArray();
        foreach (var control in targets)
        {
            writers.Add(AmongUsClient.Instance.StartRpcImmediately(playerControl.NetId, callId, SendOption.Reliable,
                control.GetClientID()));
            parts.Add($"{control.name}({control.PlayerId})");
        }

        Main.Logger.LogDebug($"Rpc {callId} sent to {string.Join(", ", parts)}");

        return new RpcWriter(writers.ToArray());
    }

    public static void StartAndSendRpc(PlayerControl player, KnownRpc rpc, PlayerControl[]? targets = null)
        => StartRpcImmediately(player, rpc, targets).Finish();

    public static void StartAndSendRpc(PlayerControl player, RpcCalls rpc, PlayerControl[]? targets = null)
        => StartRpcImmediately(player, rpc, targets).Finish();

    public static void StartAndSendRpc(KnownRpc rpc, PlayerControl[]? targets = null)
        => StartRpcImmediately(rpc, targets).Finish();

    public static void StartAndSendRpc(RpcCalls rpc, PlayerControl[]? targets = null)
        => StartRpcImmediately(rpc, targets).Finish();


    public class RpcWriter
    {
        private readonly MessageWriter[] _writers;

        internal RpcWriter(MessageWriter[] writers)
        {
            _writers = writers;
        }

        public RpcWriter WriteAll(params dynamic[] values)
        {
            foreach (var messageWriter in _writers)
                values.ForEach(v => messageWriter.Write(v));
            return this;
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

        public RpcWriter WriteVector2(Vector2 vec)
        {
            foreach (var messageWriter in _writers) NetHelpers.WriteVector2(vec, messageWriter);
            return this;
        }

        public void Finish()
        {
            foreach (var messageWriter in _writers) AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
        }

        public MessageWriter[] GetWriters() => _writers;
    }
}