using System;
using System.Linq;
using COG.Utils.Coding;

namespace COG.UI.CustomOption.ValueRules.Impl;

/// <summary>
/// 【注】请不要将其运用于职业设置，它只能用于一般的设置
/// </summary>
public class IntOptionValueRule : INumberValueRule<int>
{
    public IntOptionValueRule(int min, int step, int max, int defaultValue, NumberSuffixes suffix = NumberSuffixes.None)
    {
        Min = min;
        Step = step;
        Max = max;
        Default = defaultValue;
        SuffixType = suffix;
        Selections = Enumerable.Range(0, (Max - Min) / Step + 1).Select(n => Min + n * Step).ToArray();
        DefaultSelection = Selections.ToList().IndexOf(Default);
    }

    public int Min { get; }
    public int Step { get; }
    public int Max { get; }
    public int Default { get; }

    public NumberSuffixes SuffixType { get; }

    public int DefaultSelection { get; }
    public int[] Selections { get; }
    object[] IValueRule.Selections => Selections.Select(s => (object)s).ToArray();

    public bool IsValid(int obj)
    {
        return obj > Min && obj < Max && (obj - Min) % Step == 0;
    }

    public void Validate(ref int obj)
    {
        var tmp = obj;

        if (obj < Min)
            obj = Min;
        else if (obj > Max)
            obj = Max;
        else if ((obj - Min) % Step != 0)
            obj = Selections.OrderBy(n => Math.Abs(n - tmp)).FirstOrDefault();
    }

    public bool IsValid(object obj)
    {
        if (obj is int unboxed) return IsValid(unboxed);
        return false;
    }
}

public class FloatOptionValueRule : INumberValueRule<float>
{
    public FloatOptionValueRule(float min, float step, float max, float defaultValue,
        NumberSuffixes suffix = NumberSuffixes.None)
    {
        Min = min;
        Step = step;
        Max = max;
        Default = defaultValue;
        SuffixType = suffix;
        Selections = Enumerable.Range(0, (int)((Max - Min) / Step + 1f)).Select(n => Min + n * Step).ToArray();
        DefaultSelection = Selections.ToList().IndexOf(Default);
    }

    public float Min { get; }
    public float Step { get; }
    public float Max { get; }
    public float Default { get; }

    public NumberSuffixes SuffixType { get; }

    public int DefaultSelection { get; }
    public float[] Selections { get; }
    object[] IValueRule.Selections => Selections.Select(s => (object)s).ToArray();

    public bool IsValid(float obj)
    {
        return obj > Min && obj < Max && (obj - Min) % Step == 0;
    }

    public void Validate(ref float obj)
    {
        var tmp = obj;

        if (obj < Min)
            obj = Min;
        else if (obj > Max)
            obj = Max;
        else if ((obj - Min) % Step != 0)
            obj = Selections.OrderBy(n => Math.Abs(n - tmp)).FirstOrDefault();
    }

    public bool IsValid(object obj)
    {
        if (obj is float unboxed) return IsValid(unboxed);
        return false;
    }

    public float[] GetSelections()
    {
        return Selections;
    }
}