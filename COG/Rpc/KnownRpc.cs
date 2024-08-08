namespace COG.Rpc;

public enum KnownRpc : uint
{
    ShareRoles = 200,
    // ShareOptions = 201,
    CleanDeadBody = 201,
    UpdateOption = 202,
    SetRole = 203,

    EatBody = 204,
    SyncLovers = 205,
    Handshake = 206,
    
    Revive = 207,
    NotifySettingChange = 208,
    Mark = 209
}