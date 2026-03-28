using System;
using System.Collections.Generic;
using COG.Role;

namespace COG.Rpc.Role;

public sealed class RoleRpc : IRoleRpc
{
    private readonly CustomRole _owner;
    private Action<RoleRpcReceiveContext>? _receiveHandler;

    internal RoleRpc(CustomRole owner)
    {
        _owner = owner;
    }
    uint IRoleRpc.AllocatedId { get; set; }
    public uint AllocatedId => ((IRoleRpc)this).AllocatedId;

    void IRoleRpc.InvokeReceive(MessageReader reader)
    {
        var ctx = new RoleRpcReceiveContext(reader);
        try
        {
            _receiveHandler?.Invoke(ctx);
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError(
                $"[RoleRpc] Exception in receive handler for {_owner.GetNormalName()} " +
                $"(allocId={AllocatedId}): {ex.Message}\n{ex.StackTrace}");
        }
    }

    public RoleRpc Receive(Action<RoleRpcReceiveContext> handler)
    {
        _receiveHandler = handler;
        return this;
    }
    
    public RoleRpcBuildContext Create() => new(_owner, AllocatedId);
}

public sealed class RoleRpcBuildContext
{
    private readonly CustomRole _owner;
    private readonly uint _allocatedId;

    private readonly List<Action<RpcWriter>> _writes = [];

    private readonly List<Action> _localActions = [];

    internal RoleRpcBuildContext(CustomRole owner, uint allocatedId)
    {
        _owner       = owner;
        _allocatedId = allocatedId;
    }
    
    public RoleRpcBuildContext Write<T>(T value) where T : notnull
    {
        var captured = value;
        _writes.Add(w => RoleRpcAutoSerializer.Write(w, captured));
        return this;
    }
    public RoleRpcSendContext Send() => new(_owner, _allocatedId, _writes);
    
    public void SendByLocal() => Send().By(PlayerControl.LocalPlayer);

    public void SendBy(PlayerControl sender, PlayerControl[]? targets = null)
        => Send().By(sender, targets);
    
    public void Perform(PlayerControl? sender = null)
    {
        sender ??= PlayerControl.LocalPlayer;

        var msgWriter = MessageWriter.Get(SendOption.Reliable);
        var localRpcWriter = new RpcWriter(new[] { msgWriter });
        foreach (var write in _writes) write(localRpcWriter);

        var payloadBytes = msgWriter.ToByteArray(false);
        msgWriter.Recycle();

        var localReader = MessageReader.Get(payloadBytes);
        var rpc = RoleRpcManager.GetRpcByAllocatedId(_allocatedId);
        try
        {
            rpc?.InvokeReceive(localReader);
        }
        finally
        {
            localReader.Recycle();
        }

        Send().By(sender);
    }
}

public sealed class RoleRpcSendContext
{
    private readonly CustomRole _owner;
    private readonly uint _allocatedId;
    private readonly List<Action<RpcWriter>> _writes;
    private bool _sent;

    internal RoleRpcSendContext(CustomRole owner, uint allocatedId, List<Action<RpcWriter>> writes)
    {
        _owner       = owner;
        _allocatedId = allocatedId;
        _writes      = writes;
    }

    public void By(PlayerControl? sender = null, PlayerControl[]? targets = null)
    {
        if (_sent) return;
        _sent = true;

        sender ??= PlayerControl.LocalPlayer;

        var writer = RpcWriter.Start(sender, KnownRpc.RoleRpc, targets);
        writer.WritePacked(_owner.Id);
        writer.WritePacked(_allocatedId);

        foreach (var write in _writes)
            write(writer);

        writer.Finish();
    }
}

public sealed class RoleRpcReceiveContext
{
    private readonly MessageReader _reader;

    internal RoleRpcReceiveContext(MessageReader reader) => _reader = reader;
    
    public T Read<T>() => RoleRpcAutoSerializer.Read<T>(_reader);
}


