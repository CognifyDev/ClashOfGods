namespace COG.UI.CustomOption.ValueRules.Impl;

public class BoolOptionValueRule : IValueRule<bool>
{
    public bool[] Selections => new[] { false, true };

#pragma warning disable CS0472
    public bool IsValid(bool obj) => obj != null;
#pragma warning restore
}