using COG.Config.Impl;
using System;
using System.Linq;

namespace COG.UI.CustomOption.ValueRules.Impl;

public class StringOptionValueRule : IValueRule<string>
{
    public string[] Selections { get; }
    public Action<LanguageConfig> OnLanguageChanged { get; }

    public bool IsValid(string item) => Selections.Contains(item);
    public bool IsValid(int index) => Selections.ElementAtOrDefault(index) != null;

    public StringOptionValueRule(string[] selections, Action<LanguageConfig> onLanguageChanged)
    {
        Selections = selections;
        OnLanguageChanged = onLanguageChanged;
    }
}