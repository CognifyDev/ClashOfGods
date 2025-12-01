using AmongUs.Data;
using Assets.InnerNet;

namespace COG.UI.Announcements;

public abstract class AnnouncementBase
{
    /// <summary>
    ///     用于标识公告的唯一编号，不可重复，建议从10003开始
    /// </summary>
    public abstract int Number { get; }

    /// <summary>
    ///     公告的唯一标题，为游戏内公告黑体加粗的字
    /// </summary>
    public abstract string Title { get; }

    /// <summary>
    ///     公告的唯一副标题，为游戏内公告黑体加粗字下方的字
    /// </summary>
    public abstract string SubTitle { get; }

    /// <summary>
    ///     公告的唯一短标题，为游戏内公告左方的字
    /// </summary>
    public abstract string ShortTitle { get; }

    /// <summary>
    ///     公告的正文，为游戏内公告有实质意义的内容的字
    /// </summary>
    public abstract string Text { get; }

    /// <summary>
    ///     公告的发布日期，格式为“YYYY-MM-DDT00:00:00Z”
    /// </summary>
    public abstract string Date { get; }

    public virtual Announcement ToAnnouncement()
    {
        var result = new Announcement
        {
            Number = Number,
            Title = Title,
            SubTitle = SubTitle,
            ShortTitle = ShortTitle,
            Text = Text,
            Language = (uint)DataManager.Settings.Language.CurrentLanguage,
            Date = Date,
            Id = "ModNews"
        };

        return result;
    }
}