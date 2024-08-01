using System;
using System.Linq;
using COG.Config.Impl;

namespace COG.UI.CustomOption.ValueRules.Impl;

public class StringOptionValueRule : IValueRule<string>
{
    public StringOptionValueRule(int defaultSelection, Func<LanguageConfig, string[]> onGetSelectionText)
    {
        DefaultSelection = defaultSelection;
        OnGetSelectionText = onGetSelectionText;
    }

    public Func<LanguageConfig, string[]> OnGetSelectionText { get; }
    public string[] Selections => OnGetSelectionText(LanguageConfig.Instance);
    object[] IValueRule.Selections => Selections.Select(s => (object)s).ToArray();
    public int DefaultSelection { get; }

    public bool IsValid(string item)
    {
        return Selections.Contains(item);
    }

    public bool IsValid(int index)
    {
        return Selections.ElementAtOrDefault(index) != null;
    }
}