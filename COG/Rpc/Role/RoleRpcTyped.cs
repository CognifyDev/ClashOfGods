using System;
using COG.Role;

namespace COG.Rpc.Role;

public sealed class RoleRpc<T> : IRoleRpc where T : notnull
{
    private readonly CustomRole _owner;
    private readonly Action<T> _onPerform;
    private readonly Action<RpcWriter, T> _onSerialize;
    private readonly Func<MessageReader, T> _onDeserialize;

    uint IRoleRpc.AllocatedId { get; set; }
    
    public uint AllocatedId => ((IRoleRpc)this).AllocatedId;

    void IRoleRpc.InvokeReceive(MessageReader reader)
    {
        try
        {
            _onPerform(_onDeserialize(reader));
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError(
                $"[RoleRpc<{typeof(T).Name}>] Exception in receive handler for " +
                $"{_owner.GetNormalName()} (allocId={AllocatedId}): {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    internal RoleRpc(CustomRole owner, Action<T> onPerform)
        : this(owner, onPerform,
               (w, v) => RoleRpcAutoSerializer.Write(w, v),
               r      => RoleRpcAutoSerializer.Read<T>(r)) { }
    
    internal RoleRpc(
        CustomRole owner,
        Action<T> onPerform,
        Action<RpcWriter, T> onSerialize,
        Func<MessageReader, T> onDeserialize)
    {
        _owner         = owner;
        _onPerform     = onPerform;
        _onSerialize   = onSerialize;
        _onDeserialize = onDeserialize;
    }
    
    public void Send(T arg) => SendCore(arg, null, null);
    
    public void Send(T arg, PlayerControl? sender, PlayerControl[]? targets = null)
        => SendCore(arg, sender, targets);
    
    public void PerformAndSend(T arg, PlayerControl? sender = null, PlayerControl[]? targets = null)
    {
        _onPerform(arg);
        SendCore(arg, sender, targets);
    }
    private void SendCore(T arg, PlayerControl? sender, PlayerControl[]? targets)
    {
        sender ??= PlayerControl.LocalPlayer;
        var writer = RpcWriter.Start(sender, KnownRpc.RoleRpc, targets);
        writer.WritePacked(_owner.Id);
        writer.WritePacked(AllocatedId);
        _onSerialize(writer, arg);
        writer.Finish(); // auto Finish — no manual call needed
    }
}

public sealed class RoleRpc<T1, T2> : IRoleRpc
    where T1 : notnull
    where T2 : notnull
{
    private readonly CustomRole _owner;
    private readonly Action<T1, T2> _onPerform;
    private readonly Action<RpcWriter, T1, T2> _onSerialize;
    private readonly Func<MessageReader, (T1, T2)> _onDeserialize;

    uint IRoleRpc.AllocatedId { get; set; }

    public uint AllocatedId => ((IRoleRpc)this).AllocatedId;

    void IRoleRpc.InvokeReceive(MessageReader reader)
    {
        try
        {
            var (a, b) = _onDeserialize(reader);
            _onPerform(a, b);
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError(
                $"[RoleRpc<{typeof(T1).Name},{typeof(T2).Name}>] Exception in receive handler for " +
                $"{_owner.GetNormalName()} (allocId={AllocatedId}): {ex.Message}\n{ex.StackTrace}");
        }
    }

    internal RoleRpc(CustomRole owner, Action<T1, T2> onPerform)
        : this(owner, onPerform,
               (w, a, b) => { RoleRpcAutoSerializer.Write(w, a); RoleRpcAutoSerializer.Write(w, b); },
               r => (RoleRpcAutoSerializer.Read<T1>(r), RoleRpcAutoSerializer.Read<T2>(r))) { }

    internal RoleRpc(
        CustomRole owner,
        Action<T1, T2> onPerform,
        Action<RpcWriter, T1, T2> onSerialize,
        Func<MessageReader, (T1, T2)> onDeserialize)
    {
        _owner         = owner;
        _onPerform     = onPerform;
        _onSerialize   = onSerialize;
        _onDeserialize = onDeserialize;
    }
    
    public void Send(T1 a, T2 b) => SendCore(a, b, null, null);

    public void Send(T1 a, T2 b, PlayerControl? sender, PlayerControl[]? targets = null)
        => SendCore(a, b, sender, targets);

    public void PerformAndSend(T1 a, T2 b, PlayerControl? sender = null, PlayerControl[]? targets = null)
    {
        _onPerform(a, b);
        SendCore(a, b, sender, targets);
    }

    private void SendCore(T1 a, T2 b, PlayerControl? sender, PlayerControl[]? targets)
    {
        sender ??= PlayerControl.LocalPlayer;
        var writer = RpcWriter.Start(sender, KnownRpc.RoleRpc, targets);
        writer.WritePacked(_owner.Id);
        writer.WritePacked(AllocatedId);
        _onSerialize(writer, a, b);
        writer.Finish();
    }
}

public sealed class RoleRpc<T1, T2, T3> : IRoleRpc
    where T1 : notnull
    where T2 : notnull
    where T3 : notnull
{
    private readonly CustomRole _owner;
    private readonly Action<T1, T2, T3> _onPerform;
    private readonly Action<RpcWriter, T1, T2, T3> _onSerialize;
    private readonly Func<MessageReader, (T1, T2, T3)> _onDeserialize;

    uint IRoleRpc.AllocatedId { get; set; }
    
    public uint AllocatedId => ((IRoleRpc)this).AllocatedId;

    void IRoleRpc.InvokeReceive(MessageReader reader)
    {
        try
        {
            var (a, b, c) = _onDeserialize(reader);
            _onPerform(a, b, c);
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError(
                $"[RoleRpc<{typeof(T1).Name},{typeof(T2).Name},{typeof(T3).Name}>] Exception in receive handler for " +
                $"{_owner.GetNormalName()} (allocId={AllocatedId}): {ex.Message}\n{ex.StackTrace}");
        }
    }

    internal RoleRpc(CustomRole owner, Action<T1, T2, T3> onPerform)
        : this(owner, onPerform,
               (w, a, b, c) =>
               {
                   RoleRpcAutoSerializer.Write(w, a);
                   RoleRpcAutoSerializer.Write(w, b);
                   RoleRpcAutoSerializer.Write(w, c);
               },
               r => (
                   RoleRpcAutoSerializer.Read<T1>(r),
                   RoleRpcAutoSerializer.Read<T2>(r),
                   RoleRpcAutoSerializer.Read<T3>(r))) { }

    internal RoleRpc(
        CustomRole owner,
        Action<T1, T2, T3> onPerform,
        Action<RpcWriter, T1, T2, T3> onSerialize,
        Func<MessageReader, (T1, T2, T3)> onDeserialize)
    {
        _owner         = owner;
        _onPerform     = onPerform;
        _onSerialize   = onSerialize;
        _onDeserialize = onDeserialize;
    }
    
    public void Send(T1 a, T2 b, T3 c) => SendCore(a, b, c, null, null);

    public void Send(T1 a, T2 b, T3 c, PlayerControl? sender, PlayerControl[]? targets = null)
        => SendCore(a, b, c, sender, targets);

    public void PerformAndSend(T1 a, T2 b, T3 c, PlayerControl? sender = null, PlayerControl[]? targets = null)
    {
        _onPerform(a, b, c);
        SendCore(a, b, c, sender, targets);
    }

    private void SendCore(T1 a, T2 b, T3 c, PlayerControl? sender, PlayerControl[]? targets)
    {
        sender ??= PlayerControl.LocalPlayer;
        var writer = RpcWriter.Start(sender, KnownRpc.RoleRpc, targets);
        writer.WritePacked(_owner.Id);
        writer.WritePacked(AllocatedId);
        _onSerialize(writer, a, b, c);
        writer.Finish();
    }
}
