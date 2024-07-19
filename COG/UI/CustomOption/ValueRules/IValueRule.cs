namespace COG.UI.CustomOption.ValueRules;

public interface IValueRule
{
    public object[] Selections { get; }

    public bool IsValid(object obj);
}

public interface INumberValueRule<T> : IValueRule where T : struct
{
    public T Min { get; }
    public T Step { get; }
    public T Max { get; }
    public T Default { get; }

    public bool IsValid(T obj);

    public void Validate(ref T obj);

    public T[] GetSelections();
}