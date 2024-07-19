using System;
using System.Linq;

namespace COG.UI.CustomOption.ValueRules.Impl;

public class IntOptionValueRule : INumberValueRule<int>
{
    public int Min { get; }
    public int Step { get; }
    public int Max { get; }
    public int Default { get; }

    object[] IValueRule.Selections => Selections.Select(i => (object)i).ToArray();
    int[] Selections { get; }

    public bool IsValid(int obj) => obj > Min && obj < Max && (obj - Min) % Step == 0;
    public bool IsValid(object obj)
    {
        if (obj is int unboxed) return IsValid(unboxed);
        return false;
    }

    public void Validate(ref int obj)
    {
        int tmp = obj;

        if (obj < Min)
            obj = Min;
        else if (obj > Max)
            obj = Max;
        else if ((obj - Min) % Step != 0)
            obj = Selections.OrderBy(n => Math.Abs(n - tmp)).FirstOrDefault();
    }

    public int[] GetSelections() => Selections;

    public IntOptionValueRule(int min, int step, int max, int defaultValue)
    {
        Min = min;
        Step = step;
        Max = max;
        Default = defaultValue;
        Selections = Enumerable.Range(0, (Max - Min) / Step + 1).Select(n => Min + n * Step).ToArray();
    }
}