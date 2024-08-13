using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using COG.Game.CustomWinner.Data;
using COG.Utils;

namespace COG.Game.CustomWinner;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class CustomWinnerManager
{
    private static CustomWinnerManager? _manager;
    public static CustomWinnerManager GetManager() => _manager ??= new CustomWinnerManager();

    private readonly List<IWinnable> _winnable = new();

    private CustomWinnerManager()
    {
        WinnableData = WinnableData.Of();
    }

    public WinnableData WinnableData { get; internal set; }

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

    internal WinnableData CheckForGameEnd()
    {
        // 首先按照权重由大到小排列
        var enumerable = _winnable.Distinct().ToList();
        enumerable.Sort((first, second) => second.GetWeight().CompareTo(first.GetWeight()));

        foreach (var winnable in enumerable)
        {
            if (WinnableData.Winnable)
            {
                return WinnableData;
            }
            
            winnable.CheckWin(WinnableData);
        }

        return WinnableData;
    }
}