using System;

namespace COG.UI.ClientOption.Impl;

public class SliderClientOption : ClientOptionBase<float, SlideBar>
{
    public SliderClientOption(string translationPath, float defaultValue, float minValue, float maxValue,
        Func<float, float> onChange, Func<float, string, string>? valueUpdateTextChange = null) : base(translationPath,
        defaultValue, onChange, valueUpdateTextChange)
    {
        MinValue = minValue;
        MaxValue = maxValue;
        CurrentValue = defaultValue;
    }

    public float MinValue { get; }
    public float MaxValue { get; }
    public float CurrentValue { get; set; }
}