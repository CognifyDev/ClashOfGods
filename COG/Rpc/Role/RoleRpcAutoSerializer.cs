using System;
using System.Collections.Generic;
using COG.Role;
using InnerNet;
using UnityEngine;

namespace COG.Rpc.Role;

internal static class RoleRpcAutoSerializer
{
    private readonly struct Entry
    {
        internal readonly Action<RpcWriter, object?> Write;
        internal readonly Func<MessageReader, object> Read;

        internal Entry(Action<RpcWriter, object?> write, Func<MessageReader, object> read)
        {
            Write = write;
            Read  = read;
        }
    }

    private static readonly Dictionary<Type, Entry> Registry = new();

    static RoleRpcAutoSerializer()
    {
        // Primitives
        Add<bool>  ((w, v) => w.Write(v),         r => r.ReadBoolean());
        Add<byte>  ((w, v) => w.Write(v),         r => r.ReadByte());
        Add<sbyte> ((w, v) => w.Write(v),         r => r.ReadSByte());
        Add<int>   ((w, v) => w.WritePacked(v),   r => r.ReadPackedInt32());
        Add<uint>  ((w, v) => w.WritePacked(v),   r => r.ReadPackedUInt32());
        Add<float> ((w, v) => w.Write(v),         r => r.ReadSingle());
        Add<string>((w, v) => w.Write(v),         r => r.ReadString());

        // Byte array
        Add<byte[]>((w, v) => w.WriteBytesAndSize(v), r => (byte[])r.ReadBytesAndSize());

        // Unity
        Add<Vector2>(
            (w, v) => w.WriteVector2(v),
            r      => new Vector2(r.ReadSingle(), r.ReadSingle()));

        // Among Us network objects
        // PlayerControl is an InnerNetObject so WriteNetObject / ReadNetObject works.
        Add<PlayerControl>(
            (w, v) => w.WriteNetObject(v),
            r      => r.ReadNetObject<PlayerControl>());

        // NetworkedPlayerInfo is NOT an InnerNetObject – serialise as PlayerId byte.
        Add<NetworkedPlayerInfo>(
            (w, v) => w.Write(v.PlayerId),
            r      => GameData.Instance.GetPlayerById(r.ReadByte()));

        // CustomRole – serialise as packed role Id, deserialise via manager.
        Add<CustomRole>(
            (w, v) => w.WritePacked(v.Id),
            r      => CustomRoleManager.GetManager().GetRoleById(r.ReadPackedInt32())!);
    }

    /// <summary>Type-safe helper to register a binding and box to the internal signature.</summary>
    private static void Add<T>(Action<RpcWriter, T> write, Func<MessageReader, T> read)
        where T : notnull
    {
        Registry[typeof(T)] = new Entry(
            (w, v) => write(w, (T)v!),
            r      => read(r)
        );
    }

    public static void Register<T>(Action<RpcWriter, T> write, Func<MessageReader, T> read)
        where T : notnull
        => Add(write, read);
    
    internal static void Write<T>(RpcWriter writer, T value)
    {
        var type = typeof(T);
        if (!Registry.TryGetValue(type, out var entry))
            throw new NotSupportedException(
                $"[RoleRpc] No auto-serialiser registered for type '{type.FullName}'. " +
                $"Call {nameof(RoleRpcAutoSerializer)}.{nameof(Register)}<{type.Name}>(...) " +
                "in your plugin startup, or use an explicit onSerialize/onDeserialize delegate.");

        entry.Write(writer, value);
    }
    
    internal static T Read<T>(MessageReader reader)
    {
        var type = typeof(T);
        if (!Registry.TryGetValue(type, out var entry))
            throw new NotSupportedException(
                $"[RoleRpc] No auto-deserialiser registered for type '{type.FullName}'.");

        return (T)entry.Read(reader);
    }
}
