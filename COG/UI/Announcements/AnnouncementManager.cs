using System;
using System.Collections.Generic;
using System.Linq;
using Assets.InnerNet;
using COG.UI.Announcements.Impl;

namespace COG.UI.Announcements;

internal class AnnouncementManager
{
    private static readonly List<AnnouncementBase> _allModNews = new();
    private static bool _isInitialized;

    public static void Initialize()
    {
        if (_isInitialized) return;

        // 注册所有公告类型
        RegisterModNews(new Announcement100());

        // 按日期排序
        _allModNews.Sort((a, b) => DateTime.Compare(DateTime.Parse(b.Date), DateTime.Parse(a.Date)));
        _isInitialized = true;
    }

    public static void RegisterModNews(AnnouncementBase modNews)
    {
        _allModNews.Add(modNews);
    }

    public static List<AnnouncementBase> GetAllModNews()
    {
        if (!_isInitialized) Initialize();
        return new List<AnnouncementBase>(_allModNews);
    }

    public static List<Announcement> GetAllAnnouncements()
    {
        return GetAllModNews().Select(news => news.ToAnnouncement()).ToList();
    }
}