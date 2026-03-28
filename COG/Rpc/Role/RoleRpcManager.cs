using System.Collections.Generic;
using COG.Role;

namespace COG.Rpc.Role;

public static class RoleRpcManager
{
    private static uint _nextId = (uint)KnownRpc.RoleRpc + 1;

    private static readonly Dictionary<uint, IRoleRpc> DispatchTable = new();

    private static readonly Dictionary<int, List<(int LocalId, uint AllocatedId)>> RoleRegistry = new();
    
    internal static uint Register(CustomRole role, int localId, IRoleRpc handler)
    {
        var allocatedId = _nextId++;
        ((IRoleRpc)handler).AllocatedId = allocatedId;

        DispatchTable[allocatedId] = handler;

        if (!RoleRegistry.TryGetValue(role.Id, out var list))
            RoleRegistry[role.Id] = list = [];

        list.Add((localId, allocatedId));

        Main.Logger.LogDebug(
            $"[RoleRpcManager] Registered  {role.GetNormalName()}.localId={localId} " +
            $"→ allocatedId={allocatedId}");

        return allocatedId;
    }
    
    public static void Dispatch(PlayerControl sender, MessageReader reader)
    {
        var roleId      = reader.ReadPackedInt32();
        var allocatedId = reader.ReadPackedUInt32();

        var role = CustomRoleManager.GetManager().GetRoleById(roleId);
        if (role is null)
        {
            Main.Logger.LogWarning(
                $"[RoleRpcManager] Unknown roleId={roleId} for allocatedId={allocatedId}. Packet discarded.");
            return;
        }

        if (!DispatchTable.TryGetValue(allocatedId, out var handler))
        {
            Main.Logger.LogWarning(
                $"[RoleRpcManager] No handler for allocatedId={allocatedId} " +
                $"(role={role.GetNormalName()}). Packet discarded.");
            return;
        }

        role.DispatchRoleRpc(handler, sender, reader);
    }
    
    internal static IRoleRpc? GetRpcByAllocatedId(uint allocatedId)
        => DispatchTable.TryGetValue(allocatedId, out var h) ? h : null;
    
    public static IReadOnlyDictionary<int, List<(int LocalId, uint AllocatedId)>> GetRegistry()
        => RoleRegistry;
}
