namespace COG.Infrastructure;

public enum GameEventType
{
    PlayerMurder,
    PlayerReport,
    PlayerExile,
    PlayerDie,
    PlayerRevive,
    PlayerDisconnect,
    PlayerShapeShift,
    PlayerVent,
    PlayerTaskFinish,
    PlayerChat,
    MeetingStart,
    MeetingEnd,
    MeetingVote,
    MeetingVotingComplete,
    GameStart,
    GameEnd,
    GameCheckEnd,
    RoleAssigned,
    RoleChanged,
    AbilityUsed,
    HudUpdate,
    OptionsMenuStart,
    Custom
}
