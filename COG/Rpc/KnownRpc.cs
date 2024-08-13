namespace COG.Rpc;

public enum KnownRpc : uint
{
    ShareRoles = 200,
    CleanDeadBody = 201,
    UpdateOption = 202,
    SetRole = 203,

    SyncLovers = 204,
    Handshake = 205,
    
    Revive = 206,
    NotifySettingChange = 207,
    Mark = 208,
    RepairAllSabotages = 209
}