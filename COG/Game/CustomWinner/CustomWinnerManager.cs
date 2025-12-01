using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using COG.Game.CustomWinner.Data;
using COG.Role;
using COG.Utils;
using UnityEngine;

namespace COG.Game.CustomWinner;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class CustomWinnerManager
{
    private static CustomWinnerManager? _manager;

    private readonly List<IWinnable> _winnable = [];

    private CustomWinnerManager()
    {
        WinnableData = WinnableData.Of();
    }

    public WinnableData WinnableData { get; internal set; }

    public static CustomWinnerManager GetManager()
    {
        return _manager ??= new CustomWinnerManager();
    }

    public void RegisterCustomWinnable(IWinnable winnable)
    {
        _winnable.Add(winnable);
    }

    public void RegisterCustomWinnables(IWinnable[] winnableArray)
    {
        winnableArray.ForEach(RegisterCustomWinnable);
    }

    internal void InitForGameStart()
    {
        WinnableData = WinnableData.Of();
    }

    /* 在处理CheckWin的时候，对于WinnableData中WinnablePlayers的操作为Add或AddRange，不涉及Clear操作，否则这里会出BUG */
    internal WinnableData CheckForGameEnd()
    {
        // 按权重降序分组处理，确保高优先级先判断
        var winnerGroups = _winnable
            .GroupBy(w => w.GetWeight(), w => w)
            .OrderByDescending(g => g.Key)
            .ToList();

        foreach (var group in winnerGroups)
        {
            var isWin = false;
            var winTexts = new List<string>();

            foreach (var winnable in group)
            {
                winnable.CheckWin(WinnableData);
                var currentWin = WinnableData.Winnable;

                // 记录胜利状态及文本
                if (currentWin)
                {
                    isWin = true;
                    if (!string.IsNullOrEmpty(WinnableData.WinText))
                        winTexts.Add(WinnableData.WinText);
                }

                WinnableData.Winnable = false; // 重置状态避免污染后续检查
            }

            if (!isWin) continue;

            // 处理胜利结果
            WinnableData.Winnable = true;
            if (winTexts.Count > 1)
            {
                // 多阵营胜利的特殊处理
                WinnableData.WinColor = Color.white;
                WinnableData.WinnableCampType = CampType.Unknown;
                WinnableData.GameOverReason = GameOverReason.CrewmatesByTask;
                WinnableData.WinText = string.Join(" ", winTexts);
            }

            return WinnableData; // 高优先级胜利直接返回
        }

        return WinnableData;
    }

    public void Reset()
    {
        _winnable.Clear();
        WinnableData = WinnableData.Of();
    }
}