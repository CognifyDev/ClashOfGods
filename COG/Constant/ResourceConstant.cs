using COG.UI.MetaContext;

namespace COG.Constant;

/// <summary>
///     资源列表常量类
///     <para />
///     记录了各种常用资源位置
/// </summary>
public static class ResourceConstant
{
    static public ResourceExpandableSpriteLoader SharpWindowBackgroundSprite = new("COG.Resources.MetaContext.StatisticsBackground.png", 100f, 5, 5);
    // Buttons
    public const string CleanDeadBodyButton = "Images/Buttons/CleanDeadBody.png";
    public const string GeneralKillButton = "Images/Buttons/GeneralKill.png";
    public const string GuessButton = "Images/Buttons/Guess.png";
    public const string DispatchButton = "Images/Buttons/Dispatch.png";
    public const string RepairButton = "Images/Buttons/Repair.png";
    public const string StareButton = "Images/Buttons/Stare.png";
    public const string ExamineButton = "Images/Buttons/Examine.png";
    public const string BlockButton = "Images/Buttons/Block.png";
    public const string CheckButton = "Images/Buttons/Check.png";
    public const string GiveKillButton = "Images/Buttons/GiveKill.png"; 
    public const string ContractButton = "Images/Buttons/Contract.png";
    public const string AntidoteButton = "Images/Buttons/Antidote.png";
    public const string StoreKillButton = "Images/Buttons/StoreKill.png";
    public const string ObserveButton = "Images/Buttons/Observe.png";
    public const string DisturbButton = "Images/Buttons/Disturb.png";

    public const string ArrowImage = "Images/Arrow.png";

    // RolePreviews
    public const string DefaultRolePreview = "Images/RolePreviews/Default.png";

    public const string BgLogoSprite = "Images/COG-BG.png";
    public const string TeamLogoSprite = "Images/TeamLogo.png";
}
