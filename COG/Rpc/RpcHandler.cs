using COG.Utils;
using System;
using System.Collections.Generic;

namespace COG.Rpc;

public interface IRpcHandler
{
    public static HashSet<IRpcHandler> Handlers { get; } = new();

    public static void Register(IRpcHandler handler) => Handlers.Add(handler);

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
        OnSend = () => RpcWriter.StartAndSend(callId);
        OnReceive = _ => onPerform();
    }

    public RpcHandler(KnownRpc callId, Action onPerform) : this((byte)callId, onPerform)
    {
    }

    public RpcHandler(RpcCalls callId, Action onPerform) : this((byte)callId, onPerform)
    {
    }

    public void Perform() => OnPerform();

    public void Send() => OnSend();

    public void PerformAndSend() => Delegate.Combine(Perform, Send).DynamicInvoke();
}

#region GENERIC RPC HANDLERS

public class RpcHandler<T> : IRpcHandler where T : notnull
{
    public byte CallId { get; }
    public Action<T> OnPerform { get; }
    public Action<RpcWriter, T> OnSend { get; }
    public Action<MessageReader> OnReceive { get; }

    public RpcHandler(byte callId, Action<T> onPerform, Action<RpcWriter, T> onSend, Func<MessageReader, T> onReceive)
    {
        CallId = callId;
        OnPerform = onPerform;
        OnSend = (writer, arg) =>
        {
            onSend(writer, arg);
            writer.Finish();
        };
        OnReceive = (r) => onPerform(onReceive(r));
    }

    public RpcHandler(KnownRpc callId, Action<T> onPerform, Action<RpcWriter, T> onSend, Func<MessageReader, T> onReceive) : this((byte)callId, onPerform, onSend, onReceive)
    {
    }

    public RpcHandler(RpcCalls callId, Action<T> onPerform, Action<RpcWriter, T> onSend, Func<MessageReader, T> onReceive) : this((byte)callId, onPerform, onSend, onReceive)
    {
    }

    public void Perform(T arg) => OnPerform(arg);

    public void Send(T arg) => OnSend(RpcWriter.Start(CallId), arg);

    public void PerformAndSend(T arg) => Delegate.Combine(Perform, Send).DynamicInvoke(arg);
}

public class RpcHandler<T1, T2> : IRpcHandler where T1 : notnull where T2 : notnull
{
    public byte CallId { get; }
    public Action<T1, T2> OnPerform { get; }
    public Action<RpcWriter, T1, T2> OnSend { get; }
    public Action<MessageReader> OnReceive { get; }

    public RpcHandler(byte callId, Action<T1, T2> onPerform, Action<RpcWriter, T1, T2> onSend, Func<MessageReader, (T1, T2)> onReceive)
    {
        CallId = callId;
        OnPerform = onPerform;
        OnSend = (writer, arg1, arg2) =>
        {
            onSend(writer, arg1, arg2);
            writer.Finish();
        };
        OnReceive = (r) =>
        {
            var (arg1, arg2) = onReceive(r);
            onPerform(arg1, arg2);
        };
    }

    public RpcHandler(KnownRpc callId, Action<T1, T2> onPerform, Action<RpcWriter, T1, T2> onSend, Func<MessageReader, (T1, T2)> onReceive) : this((byte)callId, onPerform, onSend, onReceive)
    {
    }

    public RpcHandler(RpcCalls callId, Action<T1, T2> onPerform, Action<RpcWriter, T1, T2> onSend, Func<MessageReader, (T1, T2)> onReceive) : this((byte)callId, onPerform, onSend, onReceive)
    {
    }


    public void Perform(T1 arg1, T2 arg2) => OnPerform(arg1, arg2);

    public void Send(T1 arg1, T2 arg2) => OnSend(RpcWriter.Start(CallId), arg1, arg2);

    public void PerformAndSend(T1 arg1, T2 arg2) => Delegate.Combine(Perform, Send).DynamicInvoke(arg1, arg2);
}

public class RpcHandler<T1, T2, T3> : IRpcHandler where T1 : notnull where T2 : notnull where T3 : notnull
{
    public byte CallId { get; }
    public Action<T1, T2, T3> OnPerform { get; }
    public Action<RpcWriter, T1, T2, T3> OnSend { get; }
    public Action<MessageReader> OnReceive { get; }

    public RpcHandler(byte callId, Action<T1, T2, T3> onPerform, Action<RpcWriter, T1, T2, T3> onSend, Func<MessageReader, (T1, T2, T3)> onReceive)
    {
        CallId = callId;
        OnPerform = onPerform;
        OnSend = (writer, arg1, arg2, arg3) =>
        {
            onSend(writer, arg1, arg2, arg3);
            writer.Finish();
        };
        OnReceive = (r) =>
        {
            var (arg1, arg2, arg3) = onReceive(r);
            onPerform(arg1, arg2, arg3);
        };
    }

    public RpcHandler(KnownRpc callId, Action<T1, T2, T3> onPerform, Action<RpcWriter, T1, T2, T3> onSend, Func<MessageReader, (T1, T2, T3)> onReceive) : this((byte)callId, onPerform, onSend, onReceive)
    {
    }

    public RpcHandler(RpcCalls callId, Action<T1, T2, T3> onPerform, Action<RpcWriter, T1, T2, T3> onSend, Func<MessageReader, (T1, T2, T3)> onReceive) : this((byte)callId, onPerform, onSend, onReceive)
    {
    }

    public void Perform(T1 arg1, T2 arg2, T3 arg3) => OnPerform(arg1, arg2, arg3);
    public void Send(T1 arg1, T2 arg2, T3 arg3) => OnSend(RpcWriter.Start(CallId), arg1, arg2, arg3);
    public void PerformAndSend(T1 arg1, T2 arg2, T3 arg3) => Delegate.Combine(Perform, Send).DynamicInvoke(arg1, arg2, arg3);
}

public class RpcHandler<T1, T2, T3, T4> : IRpcHandler where T1 : notnull where T2 : notnull where T3 : notnull where T4 : notnull
{
    public byte CallId { get; }
    public Action<T1, T2, T3, T4> OnPerform { get; }
    public Action<RpcWriter, T1, T2, T3, T4> OnSend { get; }
    public Action<MessageReader> OnReceive { get; }

    public RpcHandler(byte callId, Action<T1, T2, T3, T4> onPerform, Action<RpcWriter, T1, T2, T3, T4> onSend, Func<MessageReader, (T1, T2, T3, T4)> onReceive)
    {
        CallId = callId;
        OnPerform = onPerform;
        OnSend = (writer, arg1, arg2, arg3, arg4) =>
        {
            onSend(writer, arg1, arg2, arg3, arg4);
            writer.Finish();
        };
        OnReceive = (r) =>
        {
            var (arg1, arg2, arg3, arg4) = onReceive(r);
            onPerform(arg1, arg2, arg3, arg4);
        };
    }

    public RpcHandler(KnownRpc callId, Action<T1, T2, T3, T4> onPerform, Action<RpcWriter, T1, T2, T3, T4> onSend, Func<MessageReader, (T1, T2, T3, T4)> onReceive) : this((byte)callId, onPerform, onSend, onReceive)
    {
    }

    public RpcHandler(RpcCalls callId, Action<T1, T2, T3, T4> onPerform, Action<RpcWriter, T1, T2, T3, T4> onSend, Func<MessageReader, (T1, T2, T3, T4)> onReceive) : this((byte)callId, onPerform, onSend, onReceive)
    {
    }


    public void Perform(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => OnPerform(arg1, arg2, arg3, arg4);
    public void Send(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => OnSend(RpcWriter.Start(CallId), arg1, arg2, arg3, arg4);
    public void PerformAndSend(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => Delegate.Combine(Perform, Send).DynamicInvoke(arg1, arg2, arg3, arg4);
}

public class RpcHandler<T1, T2, T3, T4, T5> : IRpcHandler where T1 : notnull where T2 : notnull where T3 : notnull where T4 : notnull where T5 : notnull
{
    public byte CallId { get; }
    public Action<T1, T2, T3, T4, T5> OnPerform { get; }
    public Action<RpcWriter, T1, T2, T3, T4, T5> OnSend { get; }
    public Action<MessageReader> OnReceive { get; }

    public RpcHandler(byte callId, Action<T1, T2, T3, T4, T5> onPerform, Action<RpcWriter, T1, T2, T3, T4, T5> onSend, Func<MessageReader, (T1, T2, T3, T4, T5)> onReceive)
    {
        CallId = callId;
        OnPerform = onPerform;
        OnSend = (writer, arg1, arg2, arg3, arg4, arg5) =>
        {
            onSend(writer, arg1, arg2, arg3, arg4, arg5);
            writer.Finish();
        };
        OnReceive = (r) =>
        {
            var (arg1, arg2, arg3, arg4, arg5) = onReceive(r);
            onPerform(arg1, arg2, arg3, arg4, arg5);
        };
    }

    public RpcHandler(KnownRpc callId, Action<T1, T2, T3, T4, T5> onPerform, Action<RpcWriter, T1, T2, T3, T4, T5> onSend, Func<MessageReader, (T1, T2, T3, T4, T5)> onReceive) : this((byte)callId, onPerform, onSend, onReceive)
    {
    }

    public RpcHandler(RpcCalls callId, Action<T1, T2, T3, T4, T5> onPerform, Action<RpcWriter, T1, T2, T3, T4, T5> onSend, Func<MessageReader, (T1, T2, T3, T4, T5)> onReceive) : this((byte)callId, onPerform, onSend, onReceive)
    {
    }

    public void Perform(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => OnPerform(arg1, arg2, arg3, arg4, arg5);
    public void Send(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => OnSend(RpcWriter.Start(CallId), arg1, arg2, arg3, arg4, arg5);
    public void PerformAndSend(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => Delegate.Combine(Perform, Send).DynamicInvoke(arg1, arg2, arg3, arg4, arg5);
}

#endregion