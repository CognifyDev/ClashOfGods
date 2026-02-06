namespace COG.Rpc;

public enum KnownRpc : uint
{
    ShareRoles = 100,
    HideDeadBody,
    UpdateOption,
    SetRole,
    Feedback, // ?
    Revive,
    NotifySettingChange,
    Mark,
    ShareOptions,
    ClearSabotages,
    ShareWinners,
    KillWithoutDeadBody,
    ShareAbilityOrVentUseForInspector,
    SyncRoleGameData,
    GiveOneKill,
    EnchanterPunishesKiller,
    TroubleMakerDisturb,
    NightmareStore,
    NightmareCooldownCheck,
    SpyRevealClosestTarget,
    WitchUsesAntidote,
    DieWithoutAnimationAndBody,
    SyncGameEvent,
    AdvancedMurder,
    GuessPlayer
}