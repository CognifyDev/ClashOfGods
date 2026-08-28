using COG.UI.CustomOption;
using COG.UI.CustomOption.ValueRules.Impl;

namespace COG.Role.Options;

/// <summary>
/// Type-safe wrapper for boolean options.
/// </summary>
public class BoolOption
{
    private readonly CustomOption _option;
    
    public BoolOption(CustomRole role, string key, bool defaultValue = false)
    {
        _option = CustomOption.Of(
            CustomRole.GetTabType(role),
            () => role.GetContextFromLanguage(key),
            new BoolOptionValueRule(defaultValue));
        _option.Register();
        role.AllOptions.Add(_option);
    }
    
    public bool Value => _option.GetBool();
    public static implicit operator bool(BoolOption opt) => opt.Value;
}
