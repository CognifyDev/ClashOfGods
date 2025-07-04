global using RpcWriter = COG.Utils.RpcUtils.RpcWriter;
using System;
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

    public static RpcWriter StartRpcImmediately(byte callId,
        PlayerControl[]? targets = null)
    {
        return StartRpcImmediately(PlayerControl.LocalPlayer, callId, targets);
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

    public static void StartAndSendRpc(PlayerControl player, byte rpc, PlayerControl[]? targets = null)
        => StartRpcImmediately(player, rpc, targets).Finish();

    public static void StartAndSendRpc(KnownRpc rpc, PlayerControl[]? targets = null)
        => StartRpcImmediately(rpc, targets).Finish();

    public static void StartAndSendRpc(RpcCalls rpc, PlayerControl[]? targets = null)
        => StartRpcImmediately(rpc, targets).Finish();

    public static void StartAndSendRpc(byte rpc, PlayerControl[]? targets = null)
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

    #region GENERIC RPC HANDLERS

    public interface IRpcHandler
    {
        public Action<MessageReader> OnReceive { get; }
    }

    public class RpcHandler : IRpcHandler
    {
        public byte CallId { get; }
        public Action OnPerform { get; }
        public Action OnSend { get; }
        public Action<MessageReader> OnReceive { get; } // For dynamic access

        public RpcHandler(byte callId, Action onPerform)
        {
            CallId = callId;
            OnPerform = onPerform;
            OnSend = () => StartAndSendRpc(callId);
            OnReceive = _ => onPerform();
        }

        public void Perform() => OnPerform();

        public void Send() => OnSend();

        public void PerformAndSend() => Delegate.Combine(Perform, Send).DynamicInvoke();
    }

    public class RpcHandler<T> : IRpcHandler where T : notnull
    {
        public byte CallId { get; }
        public Action<T> OnPerform { get; }
        public Action<RpcWriter, T> OnSend { get; }
        public Action<MessageReader> OnReceive { get; }

        public RpcHandler(byte callId, Action<T> onPerform, Action<RpcWriter, T> onSend, Action<MessageReader> onReceive)
        {
            CallId = callId;
            OnPerform = onPerform;
            OnSend = onSend;
            OnReceive = onReceive;
        }

        public void Perform(T arg) => OnPerform(arg);

        public void Send(T arg) => OnSend(StartRpcImmediately(CallId), arg);

        public void PerformAndSend(T arg) => Delegate.Combine(Perform, Send).DynamicInvoke(arg);
    }

    public class RpcHandler<T1, T2> : IRpcHandler where T1 : notnull where T2 : notnull
    {
        public byte CallId { get; }
        public Action<T1, T2> OnPerform { get; }
        public Action<RpcWriter, T1, T2> OnSend { get; }
        public Action<MessageReader> OnReceive { get; }

        public RpcHandler(byte callId, Action<T1, T2> onPerform, Action<RpcWriter, T1, T2> onSend, Action<MessageReader> onReceive)
        {
            CallId = callId;
            OnPerform = onPerform;
            OnSend = onSend;
            OnReceive = onReceive;
        }

        public void Perform(T1 arg1, T2 arg2) => OnPerform(arg1, arg2);

        public void Send(T1 arg1, T2 arg2) => OnSend(StartRpcImmediately(CallId), arg1, arg2);

        public void PerformAndSend(T1 arg1, T2 arg2) => Delegate.Combine(Perform, Send).DynamicInvoke(arg1, arg2);
    }

    public class RpcHandler<T1, T2, T3> : IRpcHandler where T1 : notnull where T2 : notnull where T3 : notnull
    {
        public byte CallId { get; }
        public Action<T1, T2, T3> OnPerform { get; }
        public Action<RpcWriter, T1, T2, T3> OnSend { get; }
        public Action<MessageReader> OnReceive { get; }

        public RpcHandler(byte callId, Action<T1, T2, T3> onPerform, Action<RpcWriter, T1, T2, T3> onSend, Action<MessageReader> onReceive)
        {
            CallId = callId;
            OnPerform = onPerform;
            OnSend = onSend;
            OnReceive = onReceive;
        }

        public void Perform(T1 arg1, T2 arg2, T3 arg3) => OnPerform(arg1, arg2, arg3);
        public void Send(T1 arg1, T2 arg2, T3 arg3) => OnSend(StartRpcImmediately(CallId), arg1, arg2, arg3);
        public void PerformAndSend(T1 arg1, T2 arg2, T3 arg3) => Delegate.Combine(Perform, Send).DynamicInvoke(arg1, arg2, arg3);
    }

    public class RpcHandler<T1, T2, T3, T4> : IRpcHandler where T1 : notnull where T2 : notnull where T3 : notnull where T4 : notnull
    {
        public byte CallId { get; }
        public Action<T1, T2, T3, T4> OnPerform { get; }
        public Action<RpcWriter, T1, T2, T3, T4> OnSend { get; }
        public Action<MessageReader> OnReceive { get; }

        public RpcHandler(byte callId, Action<T1, T2, T3, T4> onPerform, Action<RpcWriter, T1, T2, T3, T4> onSend, Action<MessageReader> onReceive)
        {
            CallId = callId;
            OnPerform = onPerform;
            OnSend = onSend;
            OnReceive = onReceive;
        }

        public void Perform(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => OnPerform(arg1, arg2, arg3, arg4);
        public void Send(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => OnSend(StartRpcImmediately(CallId), arg1, arg2, arg3, arg4);
        public void PerformAndSend(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => Delegate.Combine(Perform, Send).DynamicInvoke(arg1, arg2, arg3, arg4);
    }

    public class RpcHandler<T1, T2, T3, T4, T5> : IRpcHandler where T1 : notnull where T2 : notnull where T3 : notnull where T4 : notnull where T5 : notnull
    {
        public byte CallId { get; }
        public Action<T1, T2, T3, T4, T5> OnPerform { get; }
        public Action<RpcWriter, T1, T2, T3, T4, T5> OnSend { get; }
        public Action<MessageReader> OnReceive { get; }

        public RpcHandler(byte callId, Action<T1, T2, T3, T4, T5> onPerform, Action<RpcWriter, T1, T2, T3, T4, T5> onSend, Action<MessageReader> onReceive)
        {
            CallId = callId;
            OnPerform = onPerform;
            OnSend = onSend;
            OnReceive = onReceive;
        }

        public void Perform(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => OnPerform(arg1, arg2, arg3, arg4, arg5);
        public void Send(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => OnSend(StartRpcImmediately(CallId), arg1, arg2, arg3, arg4, arg5);
        public void PerformAndSend(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => Delegate.Combine(Perform, Send).DynamicInvoke(arg1, arg2, arg3, arg4, arg5);
    }

    #endregion
}