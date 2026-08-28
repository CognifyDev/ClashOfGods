using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;

namespace COG.Role.Options;

/// <summary>
/// Type-safe wrapper for float options.
/// </summary>
public class FloatOption
{
    private readonly CustomOption _option;
    
    public FloatOption(CustomRole role, string key, float min, float max, float defaultValue, float step = 1f)
    {
        _option = CustomOption.Of(
            CustomRole.GetTabType(role),
            () => role.GetContextFromLanguage(key),
            new FloatOptionValueRule(defaultValue, min, max, step));
        _option.Register();
        role.AllOptions.Add(_option);
    }
    
    public float Value => _option.GetFloat();
    public static implicit operator float(FloatOption opt) => opt.Value;
}
