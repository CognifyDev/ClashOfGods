using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.Data.Player;
using Assets.InnerNet;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace COG.UI.Announcements;

//理论基于TownOfHost_Y ：https://github.com/Yumenopai/TownOfHost_Y
[HarmonyPatch(typeof(PlayerAnnouncementData))]
public class AnnouncementsInjectorPatch
{
    [HarmonyPatch(nameof(PlayerAnnouncementData.SetAnnouncements))]
    [HarmonyPrefix]
    public static bool InjectModAnnouncements(PlayerAnnouncementData __instance,
        [HarmonyArgument(0)] ref Il2CppReferenceArray<Announcement> aRange)
    {
        try
        {
            // 获取所有mod公告
            var modAnnouncements = AnnouncementManager.GetAllAnnouncements();

            // 合并原始公告和mod公告，排除重复项
            var originalAnnouncements = aRange.ToList();
            var finalAnnouncements = new List<Announcement>();

            // 添加mod公告
            finalAnnouncements.AddRange(modAnnouncements);

            // 添加原始公告（排除与mod公告重复的）
            foreach (var original in originalAnnouncements)
                if (!modAnnouncements.Any(x => x.Number == original.Number))
                    finalAnnouncements.Add(original);

            // 按日期排序
            finalAnnouncements.Sort((a1, a2) =>
                DateTime.Compare(DateTime.Parse(a2.Date), DateTime.Parse(a1.Date)));

            // 创建新的数组
            aRange = new Il2CppReferenceArray<Announcement>(finalAnnouncements.Count);
            for (var i = 0; i < finalAnnouncements.Count; i++) aRange[i] = finalAnnouncements[i];

            return true;
        }
        catch (System.Exception ex)
        {
            Main.Logger.LogError($"Error injecting mod announcements: {ex}", "ModNewsInjector");
            return true; // 继续执行原始方法
        }
    }
}