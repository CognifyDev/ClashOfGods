using System;
using UnityEngine;

namespace COG.UI.ClientOption;

public abstract class ClientOptionBase<TValue, TGameObject> : IClientOption where TGameObject : Component
{
    public string Translatable { get; }
    public TValue DefaultValue { get; }
    Component? IClientOption.Component => OptionObject;

    public Func<TValue, TValue> OnChange { get; }
    public Func<TValue, string, string>? OnUpdateTextChange { get; }

    public TGameObject? OptionObject { get; set; }

    protected ClientOptionBase(string translatable, TValue defaultValue, Func<TValue, TValue> onChange, Func<TValue, string, string>? valueUpdateTextChange = null)
    {
        Translatable = translatable;
        DefaultValue = defaultValue;
        OnChange = onChange;
        OnUpdateTextChange = valueUpdateTextChange;
    }
}
