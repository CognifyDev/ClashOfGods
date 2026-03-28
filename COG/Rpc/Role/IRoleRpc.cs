namespace COG.Rpc.Role;

internal interface IRoleRpc
{
    uint AllocatedId { get; internal set; }
    void InvokeReceive(MessageReader reader);
}
