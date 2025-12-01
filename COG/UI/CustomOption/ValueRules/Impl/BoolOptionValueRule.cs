using System.Linq;

namespace COG.UI.CustomOption.ValueRules.Impl;

public class BoolOptionValueRule : IValueRule<bool>
{
    public bool[] Selections => [false, true];
    object[] IValueRule.Selections => Selections.Select(s => (object)s).ToArray();
    public int DefaultSelection { get; }

#pragma warning disable CS0472
    public bool IsValid(bool obj)
    {
        return obj != null;
    }
#pragma warning restore

    public BoolOptionValueRule(bool defaultSelection)
    {
        DefaultSelection = defaultSelection ? 1 : 0;
    }
}