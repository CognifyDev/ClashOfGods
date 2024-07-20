namespace COG.UI.CustomOption.ValueRules;

public interface IValueRule<T>
{
    public T[] Selections { get; }

    public bool IsValid(T obj);
}

public interface INumberValueRule<T> : IValueRule<T> where T : struct
{
    public T Min { get; }
    public T Step { get; }
    public T Max { get; }
    public T Default { get; }

    public void Validate(ref T obj);
}