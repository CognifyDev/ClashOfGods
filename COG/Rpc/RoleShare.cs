using COG.Utils;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;

namespace COG.Rpc;

[RegisterCustomRpc((uint) KnownRpc.ShareRoles)]
public abstract class RoleShare : PlayerCustomRpc<Main, RoleShare.Data>
{
    protected RoleShare(Main plugin, uint id) : base(plugin, id) {}
    
    public readonly record struct Data(Role.Role Role);

    public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;
    
    public override void Write(MessageWriter writer, Data data)
    {
        writer.Write(data.Role.GetType().Name);
    }

    public override Data Read(MessageReader reader)
    {
        return new Data(Role.RoleManager.GetManager().GetRoleByClassName(reader.ReadString())!);
    }

    public override void Handle(PlayerControl innerNetObject, Data data)
    {
        Main.Logger.LogInfo("Received player " + innerNetObject.name + "'s role " + data.Role.GetType().Name);
        GameUtils.Data.Add(innerNetObject, data.Role);
    }
}