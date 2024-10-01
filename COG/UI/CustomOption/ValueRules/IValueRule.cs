using System.Linq;

namespace COG.UI.CustomOption.ValueRules;

public interface IValueRule
{
    public int DefaultSelection { get; }
    public object[] Selections { get; }

    public bool Equals(IValueRule rule)
    {
        return rule.DefaultSelection == DefaultSelection && Selections.SequenceEqual(rule.Selections);
    }
}

public interface IValueRule<T> : IValueRule
{
#pragma warning disable CS0108
    public T[] Selections { get; }
#pragma warning restore

    public bool IsValid(T obj);
}

public interface INumberValueRule<T> : IValueRule<T> where T : struct
{
    public T Min { get; }
    public T Step { get; }
    public T Max { get; }
    public T Default { get; }

    public NumberSuffixes SuffixType { get; }

    public void Validate(ref T obj);
}