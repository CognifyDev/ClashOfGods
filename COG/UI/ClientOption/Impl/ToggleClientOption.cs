using System;

namespace COG.UI.ClientOption.Impl;

public class ToggleClientOption : ClientOptionBase<bool, ToggleButtonBehaviour>
{
    public ToggleClientOption(string translationPath, bool defaultValue, Func<bool, bool> onClick, Func<bool, string, string>? valueUpdateTextChange = null) : base(translationPath, defaultValue, onClick, valueUpdateTextChange)
    {
    }
}