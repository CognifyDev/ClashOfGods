namespace COG.Rpc;

public enum KnownRpc : uint
{
    ShareRoles = 100,
    CleanDeadBody = 101,
    UpdateOption = 102,
    SetRole = 103,

    SyncLovers = 104,
    Handshake = 105,
    
    Revive = 106,
    NotifySettingChange = 107,
    Mark = 108,
    ShareOptions = 109
}